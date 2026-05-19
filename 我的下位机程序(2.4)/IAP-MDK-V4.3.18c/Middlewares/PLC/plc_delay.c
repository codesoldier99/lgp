#include "plc_delay.h"

typedef struct
{
	uint8_t  enable;
	uint32_t time_dst;
	uint32_t time_dly;
	
	//流程端口
	uint8_t            feedback;
	PLC_TYPE_FLOW_INFO *p_flow_info;
	
	//通信端口
	uint8_t modbus_port_id;	
}PLC_DELAY_PORT;

PLC_DELAY_PORT g_plc_delay_port[CONFIG_PLC_DELAY_PORT_COUNT];

/*
*****************************************************************************************************
* 信息反馈
*****************************************************************************************************
*/
void plc_delay_feedback(uint8_t id)
{
	PLC_DELAY_PORT *p_delay_port = g_plc_delay_port + id;
	
	//反馈信息
	if(p_delay_port->feedback == CONFIG_PLC_FEEDBACK_UART){
		;
	}
	else if(p_delay_port->feedback == CONFIG_PLC_FEEDBACK_FLOW){
		PLC_TYPE_FLOW_INFO *p_flow_info = p_delay_port->p_flow_info;
		p_flow_info->stat = FALSE;
	}
}

/*
*********************************************************************************************************
* 处理延时状态
*********************************************************************************************************
*/
void plc_delay_scan(void)
{
	uint8_t     id = 0;
	PLC_DELAY_PORT *p_delay_port;
	
	for(id = 0; id < CONFIG_PLC_DELAY_PORT_COUNT; id++)
	{
		p_delay_port = g_plc_delay_port + id;
		
		if(p_delay_port->enable == TRUE){
			if(p_delay_port->time_dst <= HAL_GetTick()){			
				plc_delay_feedback(id);
				p_delay_port->enable = FALSE;
			}
		}
	}
}

/*
*****************************************************************************************************
* 逻辑控制
*****************************************************************************************************
*/
void plc_delay_flow(uint8_t id, uint32_t time_dly,\
	uint8_t feedback, uint8_t modbus_port_id,\
	PLC_TYPE_FLOW_INFO *p_flow_info, uint8_t *p_flow_pos, uint8_t flow_mask)
{
	PLC_DELAY_PORT *p_delay_port = g_plc_delay_port + id;

	//赋值
	p_delay_port->time_dly = time_dly;
	p_delay_port->time_dst = HAL_GetTick() + p_delay_port->time_dly;
	p_delay_port->feedback = feedback;
	p_delay_port->enable   = TRUE;
	p_delay_port->modbus_port_id = modbus_port_id;
	
	if(feedback == CONFIG_PLC_FEEDBACK_FLOW)
	{
		p_delay_port->p_flow_info = p_flow_info + *p_flow_pos;
		p_delay_port->p_flow_info->id   = id;
		p_delay_port->p_flow_info->port = 'T';
		p_delay_port->p_flow_info->stat = TRUE;
		
		*p_flow_pos = (*p_flow_pos + 1) & flow_mask;
	}
}
