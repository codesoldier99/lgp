#include "plc_switch.h"

/*
******************************************************************************
* 电磁阀
******************************************************************************
*/
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
	
	int8_t   input_id;
	uint8_t  input_state;
	
	//流程端口
	uint8_t            feedback;
	PLC_TYPE_FLOW_INFO *p_flow_info;
	
	//通信端口
	uint8_t modbus_port_id;
	
	//时间标记
	uint32_t time_tick;
}PLC_SWITCH_PORT;
PLC_SWITCH_PORT g_plc_switch_port[CONFIG_PLC_SWITCH_PORT_COUNT];

typedef enum{
	CONFIG_PLC_SWITCH_STEP_FREE = 0,
	CONFIG_PLC_SWITCH_STEP_WAIT_CFG,
	CONFIG_PLC_SWITCH_STEP_WAIT_SPACE,
	CONFIG_PLC_SWITCH_STEP_WAIT_RECOVER,
	CONFIG_PLC_SWITCH_STEP_WAIT_SET,
	CONFIG_PLC_SWITCH_STEP_WAIT_TRIGGER,
	CONFIG_PLC_SWITCH_STEP_WAIT_SETRECT,
}PLC_SWITCH_STEP;

typedef enum{
	CONFIG_PLC_SWITCH_RESET = 0,
	CONFIG_PLC_SWITCH_SET,
}PLC_SWITCH_STATE;


/*
*****************************************************************************************************
* 设置电磁阀开关状态
*****************************************************************************************************
*/
void plc_switch_set_port(uint8_t id, uint8_t state)
{
	
	GPIO_PinState pin_state;
	
	if(state == CONFIG_PLC_SWITCH_SET)
		pin_state = GPIO_PIN_RESET;
	else
		pin_state = GPIO_PIN_SET;
	
	switch(id)
	{
		case 0:  HAL_GPIO_WritePin(GPIOG, GPIO_PIN_0, pin_state); break;
		case 1:  HAL_GPIO_WritePin(GPIOG, GPIO_PIN_1, pin_state); break;
		case 2:  HAL_GPIO_WritePin(GPIOG, GPIO_PIN_8, pin_state); break;
		case 3:  HAL_GPIO_WritePin(GPIOG, GPIO_PIN_3, pin_state); break;
		case 4:  HAL_GPIO_WritePin(GPIOG, GPIO_PIN_4, pin_state); break;
		case 5:  HAL_GPIO_WritePin(GPIOG, GPIO_PIN_5, pin_state); break;
		case 6:  HAL_GPIO_WritePin(GPIOG, GPIO_PIN_6, pin_state); break;
		case 7:  HAL_GPIO_WritePin(GPIOG, GPIO_PIN_7, pin_state); break;
		
		case 8:  HAL_GPIO_WritePin(GPIOD, GPIO_PIN_8,  pin_state); break;
		case 9:  HAL_GPIO_WritePin(GPIOD, GPIO_PIN_9,  pin_state); break;
		case 10: HAL_GPIO_WritePin(GPIOD, GPIO_PIN_10, pin_state); break;
		case 11: HAL_GPIO_WritePin(GPIOD, GPIO_PIN_11, pin_state); break;
		case 12: HAL_GPIO_WritePin(GPIOD, GPIO_PIN_12, pin_state); break;
		case 13: HAL_GPIO_WritePin(GPIOD, GPIO_PIN_13, pin_state); break;
		case 14: HAL_GPIO_WritePin(GPIOD, GPIO_PIN_14, pin_state); break;
		case 15: HAL_GPIO_WritePin(GPIOD, GPIO_PIN_15, pin_state); break;
		default: break;
	}

	g_plc_switch_port[id].obj_state_cur = state;
}

/*
*****************************************************************************************************
* 端口初始化
*****************************************************************************************************
*/
void plc_switch_init(void)
{
	uint8_t id;
	
	for(id = 0; id < CONFIG_PLC_SWITCH_PORT_COUNT; id++){
		plc_switch_set_port(id, CONFIG_PLC_SWITCH_RESET);
	}
}


/*
*****************************************************************************************************
* 信息反馈
*****************************************************************************************************
*/
void plc_switch_feedback(uint8_t id)
{
	PLC_SWITCH_PORT *p_switch_port = g_plc_switch_port + id;
	
	if(p_switch_port->feedback == CONFIG_PLC_FEEDBACK_UART){
		mb_rtu_out(2, 17, CONFIG_PLC_MB_ADDRESS_SWITCH + id*CONFIG_PLC_SWITCH_MODBUS_LEN, CONFIG_PLC_SWITCH_MODBUS_LEN/2);
	}
	else if(p_switch_port->feedback == CONFIG_PLC_FEEDBACK_FLOW){
		PLC_TYPE_FLOW_INFO *p_flow_info = p_switch_port->p_flow_info;
		p_flow_info->stat = FALSE;
	}
}

/*
*****************************************************************************************************
* 扫描
*****************************************************************************************************
*/
void plc_switch_scan(void)
{
	uint8_t   id = 0;
  PLC_SWITCH_PORT *p_switch_port;

	for(id = 0; id < CONFIG_PLC_SWITCH_PORT_COUNT; id++)
	{
		p_switch_port = g_plc_switch_port + id;

		//待恢复
		if(p_switch_port->step == CONFIG_PLC_SWITCH_STEP_WAIT_RECOVER)
		{
			if(p_switch_port->time_tick <= HAL_GetTick())
			{				
				if(p_switch_port->obj_state_cur == CONFIG_PLC_SWITCH_SET)	//状态切换
					plc_switch_set_port(id, CONFIG_PLC_SWITCH_RESET);		
				else
					plc_switch_set_port(id, CONFIG_PLC_SWITCH_SET);
				
				p_switch_port->act_cnt_cur++;
				if(p_switch_port->act_cnt_cur >= p_switch_port->act_cnt_cfg)
				{
					p_switch_port->step = CONFIG_PLC_SWITCH_STEP_FREE;
					plc_switch_feedback(id);
				}
				else
				{
					p_switch_port->time_tick = HAL_GetTick() + p_switch_port->time_grid;
					p_switch_port->step = CONFIG_PLC_SWITCH_STEP_WAIT_SPACE;
				}		
			}	
		}
		//连续触发；间隔时间
		else if(p_switch_port->step == CONFIG_PLC_SWITCH_STEP_WAIT_SPACE)
		{
			if(p_switch_port->time_tick <= HAL_GetTick())
			{
				p_switch_port->step = CONFIG_PLC_SWITCH_STEP_WAIT_CFG;
			}
		}
		//等待设置恢复时间
		else if(p_switch_port->step == CONFIG_PLC_SWITCH_STEP_WAIT_SETRECT)
		{
			if(p_switch_port->time_hold == 0)	//无需恢复
			{
				p_switch_port->step = CONFIG_PLC_SWITCH_STEP_FREE;						
				plc_switch_feedback(id);
			}
			else							//待需恢复
			{	
				p_switch_port->time_tick = p_switch_port->time_hold + HAL_GetTick();
				p_switch_port->step = CONFIG_PLC_SWITCH_STEP_WAIT_RECOVER;	
			}
		}
		//等待传感器触发
		else if(p_switch_port->step == CONFIG_PLC_SWITCH_STEP_WAIT_TRIGGER)
		{
			if(plc_input_read_flow(p_switch_port->input_id) == p_switch_port->input_state)
				p_switch_port->step = CONFIG_PLC_SWITCH_STEP_WAIT_SETRECT;
		}
		//等待初态触发
		else if(p_switch_port->step == CONFIG_PLC_SWITCH_STEP_WAIT_SET)
		{
			if(p_switch_port->time_tick <= HAL_GetTick())
			{						
				plc_switch_set_port(id, (GPIO_PinState)p_switch_port->obj_state_cfg);
				
				if(p_switch_port->input_id == -1)		//判断是否等待传感器触发
					p_switch_port->step = CONFIG_PLC_SWITCH_STEP_WAIT_SETRECT;
				else
					p_switch_port->step = CONFIG_PLC_SWITCH_STEP_WAIT_TRIGGER;
			}
		}
		//待初始化设置		
		else if(p_switch_port->step == CONFIG_PLC_SWITCH_STEP_WAIT_CFG)	
		{
			p_switch_port->time_tick = p_switch_port->time_dly + HAL_GetTick();
			p_switch_port->step = CONFIG_PLC_SWITCH_STEP_WAIT_SET;
		}
	}
}

/*
*****************************************************************************************************
* 逻辑控制
*****************************************************************************************************
*/
void plc_switch_set_flow(uint8_t id,\
	uint8_t obj_state_cfg, uint32_t time_dly, uint32_t time_hold, uint32_t time_grid, uint8_t act_cnt_cfg,\
	int8_t  input_id, uint8_t input_state,\
	uint8_t feedback, uint8_t modbus_port_id,\
	PLC_TYPE_FLOW_INFO *p_flow_info, uint8_t *p_flow_pos, uint8_t flow_mask)
{
	PLC_SWITCH_PORT *p_switch_port = g_plc_switch_port + id;
	
	p_switch_port->time_dly       = time_dly;
	p_switch_port->time_hold      = time_hold;
	p_switch_port->time_grid      = time_grid;
	p_switch_port->act_cnt_cur    = 0;
	p_switch_port->act_cnt_cfg    = act_cnt_cfg;
	p_switch_port->input_id       = input_id;
	p_switch_port->input_state    = input_state;
	p_switch_port->obj_state_cfg  = obj_state_cfg;
	p_switch_port->step   	      = CONFIG_PLC_SWITCH_STEP_WAIT_CFG;
	p_switch_port->feedback       = feedback;
	p_switch_port->modbus_port_id = modbus_port_id;
			
	
	if(feedback == CONFIG_PLC_FEEDBACK_FLOW){
		p_switch_port->p_flow_info = p_flow_info + *p_flow_pos;
		p_switch_port->p_flow_info->id   = id;
		p_switch_port->p_flow_info->port = 'P';
		p_switch_port->p_flow_info->stat = TRUE;
		
		*p_flow_pos = (*p_flow_pos + 1) & flow_mask;
	}
}

void plc_switch_set(uint8_t id, uint8_t obj_state_cfg, uint32_t time_dly, uint32_t time_hold, uint32_t time_grid, uint8_t act_cnt_cfg)
{
	PLC_SWITCH_PORT *p_switch_port = g_plc_switch_port + id;
	
	p_switch_port->time_dly    = time_dly;
	p_switch_port->time_hold   = time_hold;
	p_switch_port->time_grid   = time_grid;
	
	p_switch_port->act_cnt_cur = 0;
	p_switch_port->act_cnt_cfg = act_cnt_cfg;
	
	p_switch_port->input_id    = -1;
	p_switch_port->input_state = 0;
	
	p_switch_port->feedback    = CONFIG_PLC_FEEDBACK_FREE;
	
	p_switch_port->obj_state_cfg = obj_state_cfg;
	p_switch_port->step          = CONFIG_PLC_SWITCH_STEP_WAIT_CFG;
}

/*
*****************************************************************************************************
* modbus
*****************************************************************************************************
*/

int8_t plc_switch_modbus_write(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask)
{
	if(id >= CONFIG_PLC_SWITCH_PORT_COUNT)
		return -1;
	
	PLC_SWITCH_PORT *p_switch_port = g_plc_switch_port + id;
	
	p_switch_port->step     = buf[(pos + 0) & mask];
	p_switch_port->feedback = buf[(pos + 1) & mask];
	
	p_switch_port->obj_state_cfg = buf[(pos + 2) & mask];
//	p_switch_port->obj_state_cur = buf[(pos + 3) & mask];
	
	p_switch_port->time_dly = (buf[(pos + 4) & mask]<<24) + (buf[(pos + 5) & mask]<<16) + (buf[(pos + 6) & mask]<<8) + buf[(pos + 7) & mask];
	p_switch_port->time_hold = (buf[(pos + 8) & mask]<<24) + (buf[(pos + 9) & mask]<<16) + (buf[(pos + 10) & mask]<<8) + buf[(pos + 11) & mask];
	p_switch_port->time_grid = (buf[(pos + 12) & mask]<<24) + (buf[(pos + 13) & mask]<<16) + (buf[(pos + 14) & mask]<<8) + buf[(pos + 15) & mask];
	
	p_switch_port->act_cnt_cur = buf[(pos + 16) & mask];
	p_switch_port->act_cnt_cfg = buf[(pos + 17) & mask];
	
	p_switch_port->input_id = buf[(pos + 18) & mask];
	p_switch_port->input_state = buf[(pos + 19) & mask];
	
	return CONFIG_PLC_SWITCH_MODBUS_LEN;
}

int8_t plc_switch_modbus_read(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask)
{
	if(id >= CONFIG_PLC_SWITCH_PORT_COUNT)
		return -1;
	
	PLC_SWITCH_PORT *p_switch_port = g_plc_switch_port + id;
	
	buf[(pos + 0) & mask] = p_switch_port->step;
	buf[(pos + 1) & mask] = p_switch_port->feedback;
	
	buf[(pos + 2) & mask] = p_switch_port->obj_state_cfg; 
	buf[(pos + 3) & mask] = p_switch_port->obj_state_cur;
	
	buf[(pos + 4) & mask] = (p_switch_port->time_dly>>24) & 0xFF;
	buf[(pos + 5) & mask] = (p_switch_port->time_dly>>16) & 0xFF;
	buf[(pos + 6) & mask] = (p_switch_port->time_dly>>8) & 0xFF;
	buf[(pos + 7) & mask] = p_switch_port->time_dly;
	
	buf[(pos + 8) & mask]  = (p_switch_port->time_hold>>24) & 0xFF;
	buf[(pos + 9) & mask]  = (p_switch_port->time_hold>>16) & 0xFF;
	buf[(pos + 10) & mask]  = (p_switch_port->time_hold>>8) & 0xFF;
	buf[(pos + 11) & mask] = p_switch_port->time_hold;
	
	buf[(pos + 12) & mask] = (p_switch_port->time_grid>>24) & 0xFF;
	buf[(pos + 13) & mask] = (p_switch_port->time_grid>>16) & 0xFF;
	buf[(pos + 14) & mask] = (p_switch_port->time_grid>>8) & 0xFF;
	buf[(pos + 15) & mask] = p_switch_port->time_grid;
	
	buf[(pos + 16) & mask] = p_switch_port->act_cnt_cur;
	buf[(pos + 17) & mask] = p_switch_port->act_cnt_cfg;
	
	buf[(pos + 18) & mask] = p_switch_port->input_id;
	buf[(pos + 19) & mask] = p_switch_port->input_state;
	
	return CONFIG_PLC_SWITCH_MODBUS_LEN;
}

