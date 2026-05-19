#include "plc_bridge.h"

typedef struct
{
	uint8_t  step;
	uint8_t  cfg_val;			//速度
	uint8_t  cur_val;
	uint8_t  dir;				//方向(正向 = 1; 反向 = 0)
	uint32_t time_dly;	//启动前延时
	uint32_t time_hold;	//可恢复时间
	
	//流程端口
	uint8_t            feedback;
	PLC_TYPE_FLOW_INFO *p_flow_info;
	
	//通信端口
	uint8_t modbus_port_id;	
	
	//时间标记
	uint32_t time_tick;
	
	uint32_t btn_time;
	uint8_t  btn_step;
	uint8_t  btn_val;
}PLC_BRIDGE_PORT;

PLC_BRIDGE_PORT g_plc_bridge_port[CONFIG_PLC_BRIDGE_PORT_COUNT];

typedef enum{
	CONFIG_PLC_BRIDGE_STEP_WAIT_FREE = 0,
	CONFIG_PLC_BRIDGE_STEP_WAIT_SETSTAT,
	CONFIG_PLC_BRIDGE_STEP_WAIT_RECOVER,
	CONFIG_PLC_BRIDGE_STEP_WAIT_TRIGGER,
}PLC_DCMOTOR_STEP;

/*
*****************************************************************************************************
* 设置输出值
*****************************************************************************************************
*/
void plc_bridge_set_value(uint8_t id, uint16_t value)
{	
	switch(id)
	{
		case 0: __HAL_TIM_SET_COMPARE(&htim3, TIM_CHANNEL_1, value); break;
		case 1: __HAL_TIM_SET_COMPARE(&htim3, TIM_CHANNEL_2, value); break;
		case 2: __HAL_TIM_SET_COMPARE(&htim3, TIM_CHANNEL_3, value); break;
		case 3: __HAL_TIM_SET_COMPARE(&htim3, TIM_CHANNEL_4, value); break;
		default: break;
	}
	
	g_plc_bridge_port[id].cur_val = value;
}


/*
*****************************************************************************************************
* 端口初始化
*****************************************************************************************************
*/
void plc_bridge_init(void)
{
	uint8_t id;
	
	for(id = 0; id < CONFIG_PLC_BRIDGE_PORT_COUNT; id++){
		switch(id){
			case 0: HAL_TIM_PWM_Start(&htim3, TIM_CHANNEL_1); break;
			case 1: HAL_TIM_PWM_Start(&htim3, TIM_CHANNEL_2);	break;
			case 2: HAL_TIM_PWM_Start(&htim3, TIM_CHANNEL_3); break;
			case 3: HAL_TIM_PWM_Start(&htim3, TIM_CHANNEL_4);	break;
			
			default: break;
		}
		plc_bridge_set_value(id, 0);
	}
}

/*
*****************************************************************************************************
* 设置方向
*****************************************************************************************************
*/
void plc_bridge_set_dir(uint8_t id, uint8_t dir)
{
	switch(id)
	{
		case 0: HAL_GPIO_WritePin(GPIOC, GPIO_PIN_3, (GPIO_PinState)dir); break;
		case 1: HAL_GPIO_WritePin(GPIOC, GPIO_PIN_2, (GPIO_PinState)dir); break;
		case 2: HAL_GPIO_WritePin(GPIOC, GPIO_PIN_1, (GPIO_PinState)dir); break;
		case 3: HAL_GPIO_WritePin(GPIOC, GPIO_PIN_0, (GPIO_PinState)dir); break;
		default: break;
	}
	
	g_plc_bridge_port[id].dir = dir;
}
/*
*****************************************************************************************************
* 信息反馈
* PLC_BRIDGE_PORT *p_bridge_port 指向具体单元
*****************************************************************************************************
*/
void plc_bridge_feedback(uint8_t id)
{
	PLC_BRIDGE_PORT *p_bridge_port = g_plc_bridge_port + id;
	
	if(p_bridge_port->feedback == CONFIG_PLC_FEEDBACK_UART){
		mb_rtu_out(2, 17, CONFIG_PLC_MB_ADDRESS_BRIDGE + id*CONFIG_PLC_BRIDGE_MODBUS_LEN, CONFIG_PLC_BRIDGE_MODBUS_LEN/2);
	}
	else if(p_bridge_port->feedback == CONFIG_PLC_FEEDBACK_FLOW){
		PLC_TYPE_FLOW_INFO *p_flow_info = p_bridge_port->p_flow_info;	
		p_flow_info->stat = FALSE;
	}
}


/*
*****************************************************************************************************
* 处理调节端口
*****************************************************************************************************
*/
void plc_bridge_scan(void)
{
	uint8_t 		 id = 0;
  PLC_BRIDGE_PORT *p_bridge_port;

	for(id = 0; id < CONFIG_PLC_BRIDGE_PORT_COUNT; id++)
	{
		p_bridge_port = g_plc_bridge_port + id;

		if(p_bridge_port->step == CONFIG_PLC_BRIDGE_STEP_WAIT_RECOVER)
		{
			if(p_bridge_port->time_tick <= HAL_GetTick()){
				plc_bridge_set_value(id, 0);	
				p_bridge_port->step = CONFIG_PLC_BRIDGE_STEP_WAIT_FREE;
				plc_bridge_feedback(id);
			}	
		}
		else if(p_bridge_port->step == CONFIG_PLC_BRIDGE_STEP_WAIT_TRIGGER)
		{
			if(p_bridge_port->time_tick <= HAL_GetTick()){
				plc_bridge_set_value(id, p_bridge_port->cfg_val);		// 设置输出
				if(p_bridge_port->time_hold == 0)	{
					p_bridge_port->step = CONFIG_PLC_BRIDGE_STEP_WAIT_FREE;
					plc_bridge_feedback(id);
				}else{	
					p_bridge_port->time_tick = p_bridge_port->time_hold + HAL_GetTick();
					p_bridge_port->step = CONFIG_PLC_BRIDGE_STEP_WAIT_RECOVER;	
				}
			}
		}
		else if(p_bridge_port->step == CONFIG_PLC_BRIDGE_STEP_WAIT_SETSTAT)	{
			plc_bridge_set_value(id, 0);	
			p_bridge_port->time_tick = p_bridge_port->time_dly + HAL_GetTick();
			p_bridge_port->step = CONFIG_PLC_BRIDGE_STEP_WAIT_TRIGGER;
			plc_bridge_set_dir(id, p_bridge_port->dir);	// 设置方向
		}
	}
}

/*
*****************************************************************************************************
* 按键响应
*****************************************************************************************************
*/
void plc_button_run_bridge(void)
{
	uint8_t id;
	PLC_BRIDGE_PORT *p_bridge_port;
	
	for(id = 0; id < CONFIG_PLC_BRIDGE_PORT_COUNT; id++){
		p_bridge_port = g_plc_bridge_port + id;	
		
		if(p_bridge_port->btn_step != CONFIG_PLC_BRIDGE_STEP_WAIT_FREE){
			plc_bridge_set_value(id, p_bridge_port->btn_val);
			
			p_bridge_port->time_tick = HAL_GetTick() + p_bridge_port->btn_time;
			p_bridge_port->step = p_bridge_port->btn_step;
			
			p_bridge_port->btn_step = CONFIG_PLC_BRIDGE_STEP_WAIT_FREE;
		}
	}
}

void plc_button_stop_bridge(void)
{
	uint8_t id;
	PLC_BRIDGE_PORT *p_bridge_port;
	
	for(id = 0; id < CONFIG_PLC_BRIDGE_PORT_COUNT; id++){
		p_bridge_port = g_plc_bridge_port + id;
		
		if(p_bridge_port->step != CONFIG_PLC_BRIDGE_STEP_WAIT_FREE){
			p_bridge_port->btn_val = p_bridge_port->cur_val;
			
			plc_bridge_set_value(id, 0);
			
			if(p_bridge_port->time_tick >= HAL_GetTick())
				p_bridge_port->btn_time = p_bridge_port->time_tick - HAL_GetTick();
			else
				p_bridge_port->btn_time = 0;
			
			p_bridge_port->btn_step = p_bridge_port->step;
			p_bridge_port->step = CONFIG_PLC_BRIDGE_STEP_WAIT_FREE;
		}
	}
}

/*
*****************************************************************************************************
* 逻辑控制
*****************************************************************************************************
*/
void plc_bridge_flow(uint8_t id, uint16_t cfg_val, uint8_t dir, uint32_t time_dly, uint32_t time_hold,\
	uint8_t feedback, uint8_t modbus_port_id,\
	PLC_TYPE_FLOW_INFO *p_flow_info, uint8_t *p_flow_pos, uint8_t flow_mask)
{
	PLC_BRIDGE_PORT *p_bridge_port = g_plc_bridge_port + id;
	
	p_bridge_port->step 	   = CONFIG_PLC_BRIDGE_STEP_WAIT_SETSTAT;	
	p_bridge_port->cfg_val   = cfg_val;
	p_bridge_port->dir    	 = dir;
	p_bridge_port->time_dly  = time_dly;
	p_bridge_port->time_hold = time_hold;
	p_bridge_port->feedback  = feedback; 	
	p_bridge_port->modbus_port_id = modbus_port_id;
	
	if(feedback == CONFIG_PLC_FEEDBACK_FLOW){
		p_bridge_port->p_flow_info = p_flow_info + *p_flow_pos;
		p_bridge_port->p_flow_info->id   = id;
		p_bridge_port->p_flow_info->port = 'm';
		p_bridge_port->p_flow_info->stat = TRUE;
		
		*p_flow_pos = (*p_flow_pos + 1) & flow_mask;
	}
}

/*
*****************************************************************************************************
* modbus
*****************************************************************************************************
*/

int8_t plc_bridge_modbus_write(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask)
{
	if(id >= CONFIG_PLC_BRIDGE_PORT_COUNT)
		return -1;
	
	PLC_BRIDGE_PORT *p_bridge_port = g_plc_bridge_port + id;
	
	p_bridge_port->step     = buf[(pos + 0) & mask];
	p_bridge_port->feedback = buf[(pos + 1) & mask];
	
	p_bridge_port->dir     = buf[(pos + 2) & mask];
	
	p_bridge_port->cfg_val = buf[(pos + 4) & mask];
	p_bridge_port->cfg_val = p_bridge_port->cfg_val <= 100 ? p_bridge_port->cfg_val : 100;
	
	p_bridge_port->time_dly = (buf[(pos + 6) & mask]<<24) + (buf[(pos + 7) & mask]<<16) + (buf[(pos + 8) & mask]<<8) + buf[(pos + 9) & mask];
	
	p_bridge_port->time_hold = (buf[(pos + 10) & mask]<<24) + (buf[(pos + 11) & mask]<<16) + (buf[(pos + 12) & mask]<<8) + buf[(pos + 13) & mask];
	
	return CONFIG_PLC_BRIDGE_MODBUS_LEN;
}

int8_t plc_bridge_modbus_read(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask)
{
	if(id >= CONFIG_PLC_BRIDGE_PORT_COUNT)
		return -1;
	
	PLC_BRIDGE_PORT *p_bridge_port = g_plc_bridge_port + id;
	
	buf[(pos + 0) & mask] = p_bridge_port->step;
	buf[(pos + 1) & mask] = p_bridge_port->feedback;
	
	buf[(pos + 2) & mask] = p_bridge_port->dir;
	
	buf[(pos + 4) & mask] = p_bridge_port->cfg_val;
	buf[(pos + 5) & mask] = p_bridge_port->cur_val;

	buf[(pos + 6) & mask] = (p_bridge_port->time_dly>>24) & 0xFF;
	buf[(pos + 7) & mask] = (p_bridge_port->time_dly>>16) & 0xFF;
	buf[(pos + 8) & mask] = (p_bridge_port->time_dly>>8) & 0xFF;
	buf[(pos + 9) & mask] = p_bridge_port->time_dly & 0xFF;
	
	buf[(pos + 10) & mask] = (p_bridge_port->time_hold>>24) & 0xFF;
	buf[(pos + 11) & mask] = (p_bridge_port->time_hold>>16) & 0xFF;
	buf[(pos + 12) & mask] = (p_bridge_port->time_hold>>8) & 0xFF;
	buf[(pos + 13) & mask] = p_bridge_port->time_hold & 0xFF;
	
	return CONFIG_PLC_BRIDGE_MODBUS_LEN;
}

