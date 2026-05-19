#include "plc_switch.h"
#include "plc_bridge.h"
#include "plc_beep.h"
#include "plc_delay.h"
#include "plc_motor.h"

#include "plc_test.h"
#include "mb_cb.h"
#include "mb_master.h"
#ifdef CONFIG_PLC_TEST_SCAN
	#define CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_SIZE	4
	#define CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK	(CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_SIZE - 1)
	typedef struct
	{
		uint8_t  step;
		PLC_TYPE_FLOW_INFO node_buf[CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_SIZE];
		uint8_t 					 node_pos;
		
		uint8_t  id;
	}PLC_FLOW_TEST_MOTOR_PORT;

	PLC_FLOW_TEST_MOTOR_PORT g_plc_flow_test_motor_port;


	#define CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_SIZE	4
	#define CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK	(CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_SIZE - 1)
	typedef struct
	{
		uint8_t  step;
		PLC_TYPE_FLOW_INFO node_buf[CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_SIZE];
		uint8_t 					 node_pos;
		
		uint8_t  id;
	}PLC_FLOW_TEST_SWITCH_PORT;
	PLC_FLOW_TEST_SWITCH_PORT g_plc_flow_test_switch_port;
	
	
	
	
	#define CONFIG_PLC_TEST_DCMOTOR_FLOW_NODE_BUF_SIZE	4
	#define CONFIG_PLC_TEST_DCMOTOR_FLOW_NODE_BUF_MASK	(CONFIG_PLC_TEST_DCMOTOR_FLOW_NODE_BUF_SIZE - 1)
	typedef struct
	{
		uint8_t  step;
		PLC_TYPE_FLOW_INFO node_buf[CONFIG_PLC_TEST_DCMOTOR_FLOW_NODE_BUF_SIZE];
		uint8_t 					 node_pos;
		
		uint8_t  id;
	}PLC_FLOW_TEST_DCMOTOR_PORT;

	PLC_FLOW_TEST_DCMOTOR_PORT g_plc_flow_test_dcmotor_port;
	
	/*自定义变量*/
	;
	uint32_t flag=0;
	/*
	***********************************************************************
	* 控制板测试
	***********************************************************************
	*/
	
	void plc_flow_test_motor_scan(void)
	{
		uint8_t id = 0;
		
		// 断点控制
		for(id = 0;  id < CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_SIZE; id++){
			if(g_plc_flow_test_motor_port.node_buf[id].stat == TRUE)
				return;
		}
//		switch(flag)
//		{
//			case 0:
//				
//								
//			}break;
//			//电机转动遇见一号光电开关停止
//			case 1:
//			
//				
//					
//			}
//				//flag=0;
//				break;
//			case 3:
//				
//			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0){
//					plc_motor_set_step_rst_flow(g_plc_flow_test_motor_port.id, 0,\
//						64000, 500, 0.1,\
//						15, 0, 0, -1,\
//						CONFIG_PLC_FEEDBACK_FLOW, 0,\
//						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);					
//			}
//					}
//		flag++;
//	
	
		switch(pusRegHoldingBuf[4])//操作modbus寄存器-不要if/else
		{
			case 1:
				if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0){
					plc_motor_set_step_rst_flow(g_plc_flow_test_motor_port.id, 0,\
						64000, 500, 0.1,\
						15, 0, 0, -1,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);}
			/*蜂鸣器函数*/
			case 2:		
				if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0){
					plc_motor_set_step_rst_flow(g_plc_flow_test_motor_port.id, 0,\
						64000, 500, 0.1,\
						14, 0, 0, 1,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);}
					pusRegHoldingBuf[4]=0;return;
			/*电机转动，不接触光电开关，发16000个脉冲*/
//			case 3:		
//				plc_motor_set_step_pos_flow(0,-16000,\
//					7500, 500, 0.4,\
//					CONFIG_PLC_FEEDBACK_FLOW, 0,\
//						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
				/*电机反方向转动*/
				
					}
		
}
		
		

	void plc_flow_test_switch_scan(void)
{
		uint8_t id = 0;
		
		// 断点控制
		for(id = 0;  id < CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_SIZE; id++){
			if(g_plc_flow_test_switch_port.node_buf[id].stat == TRUE)
				return;
		}

		switch(g_plc_flow_test_switch_port.step)
		{
			case 0:		
				plc_switch_set_flow(g_plc_flow_test_switch_port.id,\
					1, 0, 500, 500, 3,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			break;
				
			case 1:			
				plc_delay_flow(1, 1000,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			
				g_plc_flow_test_switch_port.id = (g_plc_flow_test_switch_port.id + 1)%16;
				g_plc_flow_test_switch_port.step = 0;
			return;
		}
		
		g_plc_flow_test_switch_port.step++;
	}
	
	void plc_flow_test_dcmotor_scan(void)
	{
		uint8_t id = 0;
		
		// 断点控制
		for(id = 0;  id < CONFIG_PLC_TEST_DCMOTOR_FLOW_NODE_BUF_SIZE; id++){
			if(g_plc_flow_test_dcmotor_port.node_buf[id].stat == TRUE)
				return;
		}

		switch(g_plc_flow_test_dcmotor_port.step)
		{
			case 0:					
				plc_bridge_flow(g_plc_flow_test_dcmotor_port.id, 20, 1, 0, 3000,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_dcmotor_port.node_buf, &g_plc_flow_test_dcmotor_port.node_pos, CONFIG_PLC_TEST_DCMOTOR_FLOW_NODE_BUF_MASK);
			break;
				
			case 1:			
				plc_delay_flow(2, 1000,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_dcmotor_port.node_buf, &g_plc_flow_test_dcmotor_port.node_pos, CONFIG_PLC_TEST_DCMOTOR_FLOW_NODE_BUF_MASK);
			break;
			
			case 2:					
				plc_bridge_flow(g_plc_flow_test_dcmotor_port.id, 20, 0, 0, 3000,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_dcmotor_port.node_buf, &g_plc_flow_test_dcmotor_port.node_pos, CONFIG_PLC_TEST_DCMOTOR_FLOW_NODE_BUF_MASK);
			break;
		
			case 3:
				plc_delay_flow(2, 1000,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_dcmotor_port.node_buf, &g_plc_flow_test_dcmotor_port.node_pos, CONFIG_PLC_TEST_DCMOTOR_FLOW_NODE_BUF_MASK);
			
			g_plc_flow_test_dcmotor_port.id = (g_plc_flow_test_dcmotor_port.id + 1)%3;
			g_plc_flow_test_dcmotor_port.step = 0;
			return;
		}
		
		g_plc_flow_test_dcmotor_port.step++;
	}
	
	void plc_flow_test_scan(void)
	{
		plc_flow_test_motor_scan();
		plc_flow_test_switch_scan();
		plc_flow_test_dcmotor_scan();
	}
	
#endif
