#include "plc_button.h"
#include "plc_switch.h"

#include "mb_cb.h"

typedef struct{
	uint8_t enable;
	
	uint16_t *count;
	uint16_t mark;
	
	void (*func)(void);
}PLC_BUTTON_HW_PORT;
PLC_BUTTON_HW_PORT g_plc_button_hw_port[CONFIG_PLC_BUTTON_HW_PORT_COUNT];

extern uint16_t *p_g_flow_action_enable;

/*
*****************************************************************************************************
* 回调
*****************************************************************************************************
*/
__weak void plc_button_run_input(void){;}
__weak void plc_button_run_switch(void){;}
__weak void plc_button_run_bridge(void){;}
__weak void plc_button_run_beep(void){;}
__weak void plc_button_run_delay(void){;}
__weak void plc_button_run_motor(void){;}

__weak void plc_button_stop_input(void){;}
__weak void plc_button_stop_switch(void){;}
__weak void plc_button_stop_bridge(void){;}
__weak void plc_button_stop_beep(void){;}
__weak void plc_button_stop_delay(void){;}
__weak void plc_button_stop_motor(void){;}

void plc_button_run(void)
{
	plc_button_run_input();
	plc_button_run_switch();
	plc_button_run_bridge();
	plc_button_run_beep();
	plc_button_run_delay();
	plc_button_run_motor();
	
	*p_g_flow_action_enable = 0x01;
	plc_switch_set_port(15, 1);
}

void plc_button_stop(void)
{
	plc_button_stop_input();
	plc_button_stop_switch();
	plc_button_stop_bridge();
	plc_button_stop_beep();
	plc_button_stop_delay();
	plc_button_stop_motor();
	
	*p_g_flow_action_enable = 0x00;
	plc_switch_set_port(15, 0);
}

/*
*****************************************************************************************************
* 初始化按键
*****************************************************************************************************
*/
void plc_button_init(void)
{
	uint8_t id = 0;
	
	// 硬件按键
	for(id = 0; id < CONFIG_PLC_BUTTON_HW_PORT_COUNT; id++){
		g_plc_button_hw_port[id].enable = FALSE;
	}
}

/*
*****************************************************************************************************
* 设置按键
*****************************************************************************************************
*/
void plc_button_set(uint8_t id, uint8_t input_id, void (*func)(void))
{
	PLC_BUTTON_HW_PORT *p_button_hw_port = g_plc_button_hw_port + id;
	
	p_button_hw_port->count = plc_input_count(input_id);
	p_button_hw_port->mark  = *p_button_hw_port->count;
	p_button_hw_port->func  = func;
	
	p_button_hw_port->enable = TRUE;
}

/*
*****************************************************************************************************
* 扫描
*****************************************************************************************************
*/
void plc_button_scan(void)
{
	uint8_t id;
	PLC_BUTTON_HW_PORT *p_button_hw_port;
	
	// 硬件按键
	for(id = 0; id < CONFIG_PLC_BUTTON_HW_PORT_COUNT; id++){
		p_button_hw_port = g_plc_button_hw_port + id;
		
		if(p_button_hw_port->enable == TRUE && *p_button_hw_port->count != p_button_hw_port->mark){
			p_button_hw_port->func();
			p_button_hw_port->mark = *p_button_hw_port->count;
		}
	}
	
	// 软件按键
	if(*p_g_flow_action_enable == 0x81){
		plc_button_run();
	}else if(*p_g_flow_action_enable == 0x80){
		plc_button_stop();
	}
}


