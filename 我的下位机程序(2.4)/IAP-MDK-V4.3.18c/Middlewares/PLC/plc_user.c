#include "plc_user.h"

#include "plc_flow.h"
#include "plc_test.h"
#include "plc_input.h"
#include "plc_switch.h"
#include "plc_bridge.h"
#include "plc_beep.h"
#include "plc_delay.h"
#include "plc_motor.h"
#include "plc_button.h"
#include "plc_eep.h"
#include "plc_can.h"

#include "main.h"

static uint8_t scan_count = 0;

/*
*****************************************************************
* 端口初始化
*****************************************************************
*/
void plc_user_init(void)
{
	// 初始化
	plc_input_init();
	
	// 开关量端口
	plc_switch_init();
	
	// 直流电机端口
	plc_bridge_init();
	
	// 电机端口
	plc_motor_init();
	
	// 按键
	plc_button_init();
	//plc_button_set(0, 15, plc_button_run);	//这边
	//plc_button_set(1, 14, plc_button_stop);
	
	// 存储器
	plc_eep_init(0, 0xA0, &hi2c1);
	
	// CAN
//	plc_can_init(&hcan);
	
	// 逻辑参数
	plc_flow_config();
	
	// 初始化状态灯
	plc_switch_set_port(15, 1);
}


/*
*****************************************************************
* 扫描
*****************************************************************
*/
typedef enum{
	CONFIG_PLC_SCAN_STEPMOTOR = 0,
	CONFIG_PLC_SCAN_FLOW,
	CONFIG_PLC_SCAN_INPUT,
	CONFIG_PLC_SCAN_SWITCH,
	CONFIG_PLC_SCAN_DCMOTOR,
	CONFIG_PLC_SCAN_BEEP,
	CONFIG_PLC_SCAN_DELAY,
	CONFIG_PLC_SCAN_BUTTON,
}PLC_SCAN_PORT;


uint8_t plc_user_scan(uint8_t enable)
{
	plc_motor_scan();
	
	if(enable == TRUE){
		return FALSE;
	}
	
	scan_count++;
	
	switch(scan_count)
	{
		case CONFIG_PLC_SCAN_FLOW:    
		#ifdef CONFIG_PLC_TEST_SCAN	
			plc_flow_test_scan();
		#else
			plc_flow_scan();		
		#endif
		break;
		
		case CONFIG_PLC_SCAN_INPUT:   plc_input_scan();   break;
		case CONFIG_PLC_SCAN_SWITCH:  plc_switch_scan();  break;
		case CONFIG_PLC_SCAN_DCMOTOR: plc_bridge_scan(); break;
		case CONFIG_PLC_SCAN_BEEP:    plc_beep_scan();		break;
		case CONFIG_PLC_SCAN_DELAY:   plc_delay_scan();		break;
		case CONFIG_PLC_SCAN_BUTTON:  plc_button_scan();	break;
		
		default: scan_count = 0; 
						return FALSE;
	}
		
	return TRUE;
}

