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
	
//	/*自己写的结构体*/
//	typedef struct
//{
//	uint8_t cam_state;
//	uint8_t cam_one_result;
//	uint8_t cam_two_result;
//	uint8_t cam_thr_result;
//	
//	uint32_t pull_sensor_count;
//	uint32_t pull_sensor_value;
//	uint16_t pull_sensor_flag;
//}PLC_FLOW_OBSERVED_CAMERA;

//typedef struct
//{
//	// 运行参数
//	MB_VAR_MASTER_BUF   *p_presure_sensor_buf;
//	
//	uint8_t             t_presuer_rxd_buf[4];
//	
//	uint16_t            *p_presure_sensor_get;
//	uint32_t            *p_presure_sensor_total_count;
//	uint32_t            *p_presure_sensor_ng_count;
//	int32_t            	*p_presure_sensor_value;
//	
//	int32_t              presure_sensor_offset_value;	//偏移值
//	
//	uint8_t  						step;
//	PLC_TYPE_FLOW_INFO 	node_buf[8];
//	uint8_t 					 	node_pos;
//	
//	// 变量
//	uint8_t     lift_exist_step;
//	uint32_t    lift_exist_time_mark;	// 用于判断抬举料仓空状态
//	
//	uint16_t    var_flip_ball_exist_time;	//球入槽计数
//	uint8_t     var_flip_ball_exist_count;
//	
//	// 配置参数
//	uint16_t 		cfg_motor_bldc_mix_speed;
//	uint16_t 		cfg_motor_bldc_trans_speed;
//	
//	int32_t    cfg_presure_threashold;
//	
//	// 料斗配置
//	uint16_t    cfg_motor_filp_pos_left;
//	uint16_t    cfg_motor_filp_pos_up;
//	uint16_t    cfg_motor_filp_pos_right;
//	uint16_t    cfg_motor_filp_pos_down;
//	
//}PLC_FLOW_SORT_PORT;

//PLC_FLOW_SORT_PORT g_plc_text_port;


	
	
	
	
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
//	g_plc_text_port.p_presure_sensor_total_count  = (uint32_t *)(pusRegHoldingBuf + 4);
//	g_plc_text_port.p_presure_sensor_ng_count     = (uint32_t *)(pusRegHoldingBuf + 8);
//	g_plc_text_port.p_presure_sensor_value        =  (int32_t *)(pusRegHoldingBuf + 12);
//	g_plc_text_port.p_presure_sensor_get          = (uint16_t *)(pusRegHoldingBuf + 16);
//	*g_plc_text_port.p_presure_sensor_total_count = 0;
//	*g_plc_text_port.p_presure_sensor_ng_count    = 0;
//	*g_plc_text_port.p_presure_sensor_value       = 0;
//	*g_plc_text_port.p_presure_sensor_get         = 0;

		
		
		
		
		
		switch(pusRegHoldingBuf[4])
		{
			case 1:
				plc_beep_flow(0,\
					1, 0, 200, 2000, 2,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
					break;
			case 2:		
				plc_motor_set_step_pos_flow(0,16000,\
					7500, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
				
				
		}
		
}
		
		
		/*以上程序自己写*/
//		
//		switch(g_plc_flow_test_motor_port.step)
//		{
//			case 0:	
////
//				if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0){
//					plc_motor_set_step_rst_flow(g_plc_flow_test_motor_port.id, 0,\
//						8000, 500, 0.1,\
//						1, 1, 600, 1,\
//						CONFIG_PLC_FEEDBACK_FLOW, 0,\
//						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
//				}
//				break;
//			case 1:
////				plc_delay_flow(0, 2000,\
////					CONFIG_PLC_FEEDBACK_FLOW, 0,\
////					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
////				}break;			
//			break;
//			
//			case 2:
////				
//				if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0){
//					plc_motor_set_step_rst_flow(g_plc_flow_test_motor_port.id, 0,\
//						1000, 500, 0.1,\
//						1, 1, 600, -1,\
//						CONFIG_PLC_FEEDBACK_FLOW, 0,\
//						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
//				}break;
////			g_plc_flow_test_motor_port.step=0;	
////		}
////			break;

//	//			g_plc_flow_test_switch_port.id = (g_plc_flow_test_switch_port.id + 1)%16;
//	//			g_plc_flow_test_switch_port.step = 0;
//		
//			case 3:
//		//		plc_delay_flow(0, 2000,\
//					CONFIG_PLC_FEEDBACK_FLOW, 0,\
//					g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
//			g_plc_flow_test_motor_port.step = -1;
//			//break;	
//			return;
//		}
//		
//		g_plc_flow_test_motor_port.step++;
//	}

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
