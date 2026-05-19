#include "plc_beep.h"

typedef struct
{
	uint8_t  step;			//实时状态
	
	uint8_t  obj_state_cfg; 
	uint8_t  obj_state_cur;
	
	uint32_t time_dly;	//启动前延时
	uint32_t time_hold;	//可恢复时间
	uint32_t time_grid;
	
	uint8_t  act_cnt_cur;
	uint8_t  act_cnt_cfg;
	
	//流程端口
	uint8_t            feedback;
	PLC_TYPE_FLOW_INFO *p_flow_info;
	
	//通信端口
	uint8_t modbus_port_id;
	
	//时间标记
	uint32_t time_tick;
}PLC_BEEP_PORT;

PLC_BEEP_PORT g_plc_beep_port[CONFIG_PLC_BEEP_PORT_COUNT];

#define CONFIG_SET_BEEP_RUN					1

typedef enum{
	CONFIG_PLC_BEEP_STEP_FREE = 0,
	CONFIG_PLC_BEEP_STEP_WAIT_CFG,
	CONFIG_PLC_BEEP_STEP_WAIT_SPACE,
	CONFIG_PLC_BEEP_STEP_WAIT_RECOVER,
	CONFIG_PLC_BEEP_STEP_WAIT_SET,
	CONFIG_PLC_BEEP_STEP_WAIT_SETRECT,
}PLC_BEEP_STEP;

typedef enum{
	CONFIG_PLC_BEEP_SET	= 0,
	CONFIG_PLC_BEEP_RESET,
}PLC_BEEP_STATE;

/*
*****************************************************************************************************
* 设置蜂鸣器开关状态
*****************************************************************************************************
*/
void plc_beep_set(uint8_t id, uint8_t stat)
{
	switch(id)
	{
		case 0: HAL_GPIO_WritePin(GPIOC, GPIO_PIN_5, (GPIO_PinState)stat); break;
		
		default: break;
	}
	
	g_plc_beep_port[id].obj_state_cur = stat;
}


/*
*****************************************************************************************************
* 信息反馈
* PLC_BEEP_PORT *p_beep_port 指向具体单元
*****************************************************************************************************
*/
void plc_beep_feedback(uint8_t id)
{
	PLC_BEEP_PORT *p_beep_port = g_plc_beep_port + id;
	
	if(p_beep_port->feedback == CONFIG_PLC_FEEDBACK_UART){
		mb_rtu_out(2, 17, CONFIG_PLC_MB_ADDRESS_BEEP + id*CONFIG_PLC_BEEP_MODBUS_LEN, CONFIG_PLC_BEEP_MODBUS_LEN/2);
	}
	else if(p_beep_port->feedback == CONFIG_PLC_FEEDBACK_FLOW){
		PLC_TYPE_FLOW_INFO *p_flow_info = p_beep_port->p_flow_info;
		
		p_flow_info->stat = FALSE;
	}
}

/*
*****************************************************************************************************
* 处理蜂鸣器开关状态
*****************************************************************************************************
*/
void plc_beep_scan(void)
{
	uint8_t   id = 0;
  PLC_BEEP_PORT *p_beep_port;

	for(id = 0; id < CONFIG_PLC_BEEP_PORT_COUNT; id++)
	{
		p_beep_port = g_plc_beep_port + id;

		//待恢复
		if(p_beep_port->step == CONFIG_PLC_BEEP_STEP_WAIT_RECOVER)
		{
			if(p_beep_port->time_tick <= HAL_GetTick())
			{				
				if(p_beep_port->obj_state_cur == CONFIG_PLC_BEEP_SET)	//状态切换
					plc_beep_set(id, CONFIG_PLC_BEEP_RESET);		
				else
					plc_beep_set(id, CONFIG_PLC_BEEP_SET);
				
				p_beep_port->act_cnt_cur++;
				if(p_beep_port->act_cnt_cur >= p_beep_port->act_cnt_cfg){
					p_beep_port->step = CONFIG_PLC_BEEP_STEP_FREE;
					plc_beep_feedback(id);
				}
				else{
					p_beep_port->time_tick = HAL_GetTick() + p_beep_port->time_grid;
					p_beep_port->step = CONFIG_PLC_BEEP_STEP_WAIT_SPACE;
				}		
			}	
		}
		//连续触发；间隔时间
		else if(p_beep_port->step == CONFIG_PLC_BEEP_STEP_WAIT_SPACE)
		{
			if(p_beep_port->time_tick <= HAL_GetTick()){
				p_beep_port->step = CONFIG_PLC_BEEP_STEP_WAIT_CFG;
			}
		}
		//等待设置恢复时间
		else if(p_beep_port->step == CONFIG_PLC_BEEP_STEP_WAIT_SETRECT)
		{
			if(p_beep_port->time_hold == 0) {	//无需恢复
				p_beep_port->step = CONFIG_PLC_BEEP_STEP_FREE;						
				plc_beep_feedback(id);
			}
			else {					//待需恢复
				p_beep_port->time_tick = p_beep_port->time_hold + HAL_GetTick();
				p_beep_port->step = CONFIG_PLC_BEEP_STEP_WAIT_RECOVER;	
			}
		}
		//等待初态触发
		else if(p_beep_port->step == CONFIG_PLC_BEEP_STEP_WAIT_SET)
		{
			if(p_beep_port->time_tick <= HAL_GetTick()){						
				plc_beep_set(id, p_beep_port->obj_state_cfg);
				p_beep_port->step = CONFIG_PLC_BEEP_STEP_WAIT_SETRECT;
			}
		}
		//待初始化设置		
		else if(p_beep_port->step == CONFIG_PLC_BEEP_STEP_WAIT_CFG)	
		{
			p_beep_port->time_tick = p_beep_port->time_dly + HAL_GetTick();
			p_beep_port->step = CONFIG_PLC_BEEP_STEP_WAIT_SET;
		}
	}
}

/*
*****************************************************************************************************
* 设置蜂鸣器
*****************************************************************************************************
*/
void plc_beep_flow(uint8_t id,\
	uint8_t obj_state_cfg, uint32_t time_dly, uint32_t time_hold, uint32_t time_grid, uint8_t act_cnt_cfg,\
	uint8_t feedback, uint8_t modbus_port_id,\
	PLC_TYPE_FLOW_INFO *p_flow_info, uint8_t *p_flow_pos, uint8_t flow_mask)
{
	PLC_BEEP_PORT *p_beep_port = g_plc_beep_port + id;
	
	p_beep_port->time_dly 	 = time_dly;
	p_beep_port->time_hold   = time_hold;
	p_beep_port->time_grid   = time_grid;
	
	p_beep_port->act_cnt_cur = 0;
	p_beep_port->act_cnt_cfg = act_cnt_cfg;
	
	p_beep_port->feedback 	= feedback;
	
	p_beep_port->obj_state_cfg = obj_state_cfg;
	p_beep_port->step   			 = CONFIG_PLC_BEEP_STEP_WAIT_CFG;
	
	p_beep_port->modbus_port_id = modbus_port_id;
			
	if(feedback == CONFIG_PLC_FEEDBACK_FLOW){
		p_beep_port->p_flow_info = p_flow_info + *p_flow_pos;
		p_beep_port->p_flow_info->id   = id;
		p_beep_port->p_flow_info->port = 'B';
		p_beep_port->p_flow_info->stat = TRUE;
		
		*p_flow_pos = (*p_flow_pos + 1) & flow_mask;
	}
}

/*
*****************************************************************************************************
* modbus
*****************************************************************************************************
*/

int8_t plc_beep_modbus_write(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask)
{
	if(id >= CONFIG_PLC_BEEP_PORT_COUNT)
		return -1;
	
	PLC_BEEP_PORT *p_beep_port = g_plc_beep_port + id;
	
	p_beep_port->step     = buf[(pos + 0) & mask];
	p_beep_port->feedback = buf[(pos + 1) & mask];
	
	p_beep_port->obj_state_cfg = buf[(pos + 2) & mask]; 
	p_beep_port->obj_state_cur = buf[(pos + 3) & mask];
	
	p_beep_port->time_dly = (buf[(pos + 4) & mask]<<24) + (buf[(pos + 5) & mask]<<16) + (buf[(pos + 6) & mask]<<8) + buf[(pos + 7) & mask];
	p_beep_port->time_hold = (buf[(pos + 8) & mask]<<24) + (buf[(pos + 9) & mask]<<16) + (buf[(pos + 10) & mask]<<8) + buf[(pos + 11) & mask];

	p_beep_port->time_grid = (buf[(pos + 12) & mask]<<24) + (buf[(pos + 13) & mask]<<16) + (buf[(pos + 14) & mask]<<8) + buf[(pos + 15) & mask];
	
	p_beep_port->act_cnt_cur = buf[(pos + 16) & mask];
	p_beep_port->act_cnt_cfg = buf[(pos + 17) & mask];
	
	return CONFIG_PLC_BEEP_MODBUS_LEN;
}

int8_t plc_beep_modbus_read(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask)
{
	if(id >= CONFIG_PLC_BEEP_PORT_COUNT)
		return -1;
	
	PLC_BEEP_PORT *p_beep_port = g_plc_beep_port + id;
	
	buf[(pos + 0) & mask] = p_beep_port->step;
	buf[(pos + 1) & mask] = p_beep_port->feedback;
	
	buf[(pos + 2) & mask] = p_beep_port->obj_state_cfg; 
	buf[(pos + 3) & mask] = p_beep_port->obj_state_cur;
	
	buf[(pos + 4) & mask] = (p_beep_port->time_dly>>24) & 0xFF;
	buf[(pos + 5) & mask] = (p_beep_port->time_dly>>16) & 0xFF;
	buf[(pos + 6) & mask] = (p_beep_port->time_dly>>8) & 0xFF;
	buf[(pos + 7) & mask] = p_beep_port->time_dly;
	
	buf[(pos + 8) & mask]  = (p_beep_port->time_hold>>24) & 0xFF;
	buf[(pos + 9) & mask]  = (p_beep_port->time_hold>>16) & 0xFF;
	buf[(pos + 10) & mask]  = (p_beep_port->time_hold>>8) & 0xFF;
	buf[(pos + 11) & mask] = p_beep_port->time_hold;
	
	buf[(pos + 12) & mask] = (p_beep_port->time_grid>>24) & 0xFF;
	buf[(pos + 13) & mask] = (p_beep_port->time_grid>>16) & 0xFF;
	buf[(pos + 14) & mask] = (p_beep_port->time_grid>>8) & 0xFF;
	buf[(pos + 15) & mask] = p_beep_port->time_grid;
	
	buf[(pos + 16) & mask] = p_beep_port->act_cnt_cur;
	buf[(pos + 17) & mask] = p_beep_port->act_cnt_cfg;
	
	return CONFIG_PLC_BEEP_MODBUS_LEN;
}


