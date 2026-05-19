
#include "plc_input.h"

typedef struct
{
	uint8_t  step;
	uint32_t time_hold;	//计数时长
	uint16_t count;			//触发技术
	uint8_t  state;			//电平状态
	
	//流程端口
	uint8_t            feedback;
	PLC_TYPE_FLOW_INFO *p_flow_info;
	
	//通信端口
	uint8_t modbus_port_id;	
	
	//时间标记
	uint32_t time_tick;
}PLC_INPUT_PORT;

PLC_INPUT_PORT g_plc_input_port[CONFIG_PLC_INPUT_PORT_COUNT];

typedef enum{
	CONFIG_PLC_INPUT_STEP_FREE = 0,
	CONFIG_PLC_INPUT_STEP_CFG,
	CONFIG_PLC_INPUT_STEP_WAIT,
	CONFIG_PLC_INPUT_STEP_FINISH,
}PLC_INPUT_STEP;

/*
*****************************************************************************************************
* 写入状态
*****************************************************************************************************
*/
inline void plc_input_write(uint8_t id, uint8_t state)
{
	g_plc_input_port[id].state = state;
	g_plc_input_port[id].count++;
}

/*
*****************************************************************************************************
* 读出状态
*****************************************************************************************************
*/

inline uint8_t plc_input_read(int8_t id)
{
	return g_plc_input_port[id].state;
}

uint8_t plc_input_read_flow(int8_t id)
{
	if(id == 0)
		return HAL_GPIO_ReadPin(GPIOD, GPIO_PIN_0);
	else if(id == 1)
		return HAL_GPIO_ReadPin(GPIOD, GPIO_PIN_1);
	else if(id == 2)
		return HAL_GPIO_ReadPin(GPIOG, GPIO_PIN_2);
	else if(id == 3)
		return HAL_GPIO_ReadPin(GPIOD, GPIO_PIN_3);
	else if(id == 4)
		return HAL_GPIO_ReadPin(GPIOD, GPIO_PIN_4);
	else if(id == 5)
		return HAL_GPIO_ReadPin(GPIOD, GPIO_PIN_5);
	else if(id == 6)
		return HAL_GPIO_ReadPin(GPIOD, GPIO_PIN_6);
	else if(id == 7)
		return HAL_GPIO_ReadPin(GPIOD, GPIO_PIN_7);
	else if(id == 8)
		return HAL_GPIO_ReadPin(GPIOA, GPIO_PIN_8);
	else if(id == 9)
		return HAL_GPIO_ReadPin(GPIOG, GPIO_PIN_9);
	else if(id == 10)
		return HAL_GPIO_ReadPin(GPIOG, GPIO_PIN_10);
	else if(id == 11)
		return HAL_GPIO_ReadPin(GPIOG, GPIO_PIN_11);
	else if(id == 12)
		return HAL_GPIO_ReadPin(GPIOG, GPIO_PIN_12);
	else if(id == 13)
		return HAL_GPIO_ReadPin(GPIOG, GPIO_PIN_13);
	else if(id == 14)
		return HAL_GPIO_ReadPin(GPIOG, GPIO_PIN_14);
	else if(id == 15)
		return HAL_GPIO_ReadPin(GPIOG, GPIO_PIN_15);
	
	return FALSE;
}

/*
*****************************************************************************************************
* 初始化
*****************************************************************************************************
*/
void plc_input_init(void)
{
	uint8_t id = 0;
	
	for(id = 0; id < CONFIG_PLC_INPUT_PORT_COUNT; id++){
		g_plc_input_port[id].state = plc_input_read_flow(id);
	}
}

/*
*****************************************************************************************************
* 读出数值
*****************************************************************************************************
*/
uint16_t *plc_input_count(int8_t id)
{
	return &g_plc_input_port[id].count;
}


uint16_t plc_input_count_two(int8_t id)
{
	return g_plc_input_port[id].count;
}
/*
*****************************************************************************************************
* 信息反馈

*****************************************************************************************************
*/
void plc_input_feedback(uint8_t id)
{
	PLC_INPUT_PORT *p_input_port = g_plc_input_port + id;
	
	if(p_input_port->feedback == CONFIG_PLC_FEEDBACK_UART){
		mb_rtu_out(2, 17, CONFIG_PLC_MB_ADDRESS_INPUT + id*CONFIG_PLC_INPUT_MODBUS_LEN, CONFIG_PLC_INPUT_MODBUS_LEN/2);
	}
	else if(p_input_port->feedback == CONFIG_PLC_FEEDBACK_FLOW){
		PLC_TYPE_FLOW_INFO *p_flow_info = p_input_port->p_flow_info;
		
		p_flow_info->stat = FALSE;
	}
}

/*
*****************************************************************************************************
* 扫描
*****************************************************************************************************
*/
void plc_input_scan(void)
{
	uint8_t id = 0;
	PLC_INPUT_PORT *p_input_port;
	
	for(id = 0; id < CONFIG_PLC_INPUT_PORT_COUNT; id++)
	{
		p_input_port = g_plc_input_port + id;
		
		if(p_input_port->step == CONFIG_PLC_INPUT_STEP_CFG){
			if(p_input_port->time_hold == 0){
				p_input_port->step = CONFIG_PLC_INPUT_STEP_FINISH;	
			}else{
				p_input_port->time_tick = p_input_port->time_hold + HAL_GetTick();
				p_input_port->step = CONFIG_PLC_INPUT_STEP_WAIT;
			}
		}else if(p_input_port->step == CONFIG_PLC_INPUT_STEP_WAIT){
			if(p_input_port->time_tick <= HAL_GetTick()){
				p_input_port->step = CONFIG_PLC_INPUT_STEP_FINISH;
			}
		}else if(p_input_port->step == CONFIG_PLC_INPUT_STEP_FINISH){
			p_input_port->step = CONFIG_PLC_INPUT_STEP_FREE;	
			plc_input_feedback(id);
		}
	}
}


/*
*****************************************************************************************************
* modbus
*****************************************************************************************************
*/
int8_t plc_input_modbus_write(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask)
{
	if(id >= CONFIG_PLC_INPUT_PORT_COUNT)
		return -1;
	
	PLC_INPUT_PORT *p_input_port = g_plc_input_port + id;
	
	p_input_port->step     = buf[(pos + 0) & mask];
	p_input_port->feedback = buf[(pos + 1) & mask];
	
	p_input_port->time_hold  = (buf[(pos + 4) & mask]<<24) + (buf[(pos + 5) & mask]<<16) + (buf[(pos + 6) & mask]<<8) + buf[(pos + 7) & mask];
	
	p_input_port->count = (buf[(pos + 8) & mask]<<8) | buf[(pos + 9) & mask];
	
	p_input_port->feedback = CONFIG_PLC_FEEDBACK_UART;
	
	return CONFIG_PLC_INPUT_MODBUS_LEN;
}

int8_t plc_input_modbus_read(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask)
{
	if(id >= CONFIG_PLC_INPUT_PORT_COUNT)
		return -1;
	
	PLC_INPUT_PORT *p_input_port = g_plc_input_port + id;
	
	buf[(pos + 0) & mask] = p_input_port->step;
	buf[(pos + 1) & mask] = p_input_port->feedback;
	
	buf[(pos + 2) & mask] = p_input_port->state;
	
	buf[(pos + 4) & mask] = (p_input_port->time_hold>>24) & 0xFF;
	buf[(pos + 5) & mask] = (p_input_port->time_hold>>16) & 0xFF;
	buf[(pos + 6) & mask] = (p_input_port->time_hold>>8)  & 0xFF;
	buf[(pos + 7) & mask] =  p_input_port->time_hold & 0xFF;
	
	buf[(pos + 8) & mask] = (p_input_port->count>>8) & 0xFF;
	buf[(pos + 9) & mask] =  p_input_port->count & 0xFF;
	
	return CONFIG_PLC_INPUT_MODBUS_LEN;
}

