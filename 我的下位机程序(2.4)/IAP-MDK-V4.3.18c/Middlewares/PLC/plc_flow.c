#include "plc_flow.h"

#include "plc_switch.h"
#include "plc_bridge.h"
#include "plc_beep.h"
#include "plc_delay.h"
#include "plc_motor.h"
#include "plc_eep.h"

#include "mb_master.h"
#include "mb_cb.h"
/*
*************************************************************************************
* 端口定义
*************************************************************************************
*/
typedef enum
{
	MOTOR_ASDA_ONE   = 0,
	MOTOR_ASDA_TWO   = 1,
	MOTOR_STEP_FILP  = 2,		//M3
	MOTOR_BLDC_MIX   = 3,		//M4
	MOTOR_BLDC_TRANS = 4,		//M5
	MOTOR_BLDC_TWIST = 5,		//M6
}PLC_HW_STEPMOTOR;

typedef enum
{
	MOTOR_BLDC_DIR_N = 0,
	MOTOR_BLDC_DIR_P,
}PLC_MOTOR_BLDC_DIR;

typedef enum
{
	INPUT_STATUS_ON = 0,
	INPUT_STATUS_OFF = 1,
}PLC_INPUT_STATUS;
typedef enum
{
	INPUT_MOTOR_DISK_ONE 		= 0,
	INPUT_MOTOR_DISK_TWO 		= 1,
	
	INPUT_CYL_LIFT_EXIST 		= 2,	//托举气缸
	INPUT_CYL_LIFT_UP    		= 3,
	INPUT_CYL_LIFT_DOWN  		= 4,
	
	INPUT_MOTOR_FILP_RESET 	= 5,	//翻转
	INPUT_MOTOR_FILP_EXIST 	= 6,
	
	INPUT_MOTOR_FILP_ERROR 			= 7,	//翻转错误
	
	INPUT_CYL_OK_FRONT 			= 8,
	INPUT_CYL_OK_BACK  			= 9,
	
	INPUT_CYL_NG_FRONT 			= 10,
	INPUT_CYL_NG_BACK  			= 11,
}PLC_HW_INPUT;


typedef enum
{
	SWITCH_STATUS_OFF = 0,
	SWITCH_STATUS_ON,
}PLC_SWITCH_STATUS;
typedef enum
{
	SWITCH_HOLE_ONE   = 0,
	SWITCH_HOLE_TWO		= 1,
	SWITCH_HOLE_THREE	= 2,
	SWITCH_HOLE_FOUR	= 3,
	SWITCH_HOLE_FIVE	= 4,
	SWITCH_HOLE_SIX		= 5,
	SWITCH_HOLE_SEVEN	= 6,
	SWITCH_HOLE_EIGHT	= 7,
	
	SWITCH_CYL_PRESSURE = 8,
	SWITCH_CYL_LIFT			= 9,
	
	SWITCH_CYL_NG	= 10,
	SWITCH_CYL_OK	= 11,
	
	SWITCH_TRANS_CYL = 12,
	SWITCH_TRANS_AIR = 13,
}PLC_HW_SWITCH;


typedef enum
{
	DELAY_FLOW_SORT = 0,
	DELAY_FLOW_OBSERVED,
}PLC_HW_DELAY;



/*
*************************************************************************************
* 流程全局变量
*************************************************************************************
*/

#define CONFIG_FLOW_GLOBAL_PORT_BALL_CNT		2
#define CONFIG_FLOW_GLOBAL_PORT_BALL_MAX		3
typedef struct
{
	uint8_t ball_count;	//进料求个数记录
}PLC_FLOW_GLOBAL_PORT;

PLC_FLOW_GLOBAL_PORT g_plc_flow_global_port;

uint16_t *p_g_flow_action_enable;

/*
*************************************************************************************
* 检测流程（检测）
*************************************************************************************
*/

#define CONFIG_FLOW_OBSERVED_NODE_BUF_SIZE		16
#define CONFIG_FLOW_OBSERVED_NODE_BUF_MASK		(CONFIG_FLOW_OBSERVED_NODE_BUF_SIZE - 1)

#define CONFIG_FLOW_OBSERVED_ONE_HOLE_COUNT		8
#define CONFIG_FLOW_OBSERVED_ONE_HOLE_MASK		(CONFIG_FLOW_OBSERVED_ONE_HOLE_COUNT - 1)
#define CONFIG_FLOW_OBSERVED_TWO_HOLE_COUNT		8
#define CONFIG_FLOW_OBSERVED_TWO_HOLE_MASK		(CONFIG_FLOW_OBSERVED_TWO_HOLE_COUNT - 1)

#define CONFIG_FLOW_OBSERVED_CAM_ONE_OFFSET		6
#define CONFIG_FLOW_OBSERVED_CAM_TWO_OFFSET		6
#define CONFIG_FLOW_OBSERVED_CAM_THR_OFFSET		4
#define CONFIG_FLOW_OBSERVED_ONE_NG_OFFSET		5
#define CONFIG_FLOW_OBSERVED_ONE_OK_OFFSET		4
#define CONFIG_FLOW_OBSERVED_TWO_NG_OFFSET		2
#define CONFIG_FLOW_OBSERVED_TWO_OK_OFFSET		1

typedef enum
{
	HOLE_STATE_FREE,
	HOLE_STATE_EXIST,
	
	CAM_STATE_FREE,
	CAM_STATE_READY,
	
	BALL_STATE_OK,
	BALL_STATE_NG,
}PLC_FLOW_OBSERVED_HOLE_STATE;

typedef struct
{
	uint8_t cam_state;
	uint8_t cam_one_result;
	uint8_t cam_two_result;
	uint8_t cam_thr_result;
	
	uint32_t pull_sensor_count;
	uint32_t pull_sensor_value;
	uint16_t pull_sensor_flag;
}PLC_FLOW_OBSERVED_CAMERA;

typedef struct
{
	// 运行参数
	PLC_FLOW_OBSERVED_CAMERA *p_camera_state;
	
	uint8_t             *p_disk_one_hold_id;
	uint8_t             *p_disk_one_hole_state;
	
	uint8_t             *p_disk_two_hold_id;
	uint8_t             *p_disk_two_hole_state;
	
	uint8_t  						step;
	PLC_TYPE_FLOW_INFO 	node_buf[CONFIG_FLOW_OBSERVED_NODE_BUF_SIZE];
	uint8_t 					 	node_pos;
	
	// 变量
	uint8_t 	var_lift_err_act_count;
	uint32_t 	var_lift_err_irq_mark;	
	
	// 配置参数
	uint8_t     cfg_sys_poweron_enable;
	
	// 转盘配置
	int32_t cfg_disk_one_offset;
	int32_t cfg_disk_one_count;
	int32_t cfg_disk_one_grid;
	int32_t cfg_disk_two_offset;
	int32_t cfg_disk_two_count;
	int32_t cfg_disk_two_grid;
	
	int16_t cfg_motor_bldc_twist_speed;
	
	// 抬举气缸
	uint16_t cfg_cyl_lift_up_delay;
	uint16_t cfg_cyl_lift_down_delay;
	
}PLC_FLOW_OBSERVED_PORT;

PLC_FLOW_OBSERVED_PORT g_plc_flow_observed_port;

typedef enum	//流程步骤
{
	FLOW_OBSERVED_STEP_RST_OUT_INPUT = 0,
	FLOW_OBSERVED_STEP_RST_IN_INPUT,
	FLOW_OBSERVED_STEP_RST_CLR_NG_CYL_BACK,
	FLOW_OBSERVED_STEP_RST_CLR_NG_CYL_FRONT,
	FLOW_OBSERVED_STEP_RST_CLR_NG_MOTOR_MOVE,
	
	FLOW_OBSERVED_STEP_RUN_OBJ_CLASS,
	FLOW_OBSERVED_STEP_RUN_CYL_LIFT_UP_DELAY,
	FLOW_OBSERVED_STEP_RUN_CYL_LIFT_DOWN,
	FLOW_OBSERVED_STEP_RUN_CYL_LIFT_DOWN_DELAY,
	FLOW_OBSERVED_STEP_RUN_CYL_LIFT_DOWN_CHECK,
	FLOW_OBSERVED_STEP_RUN_WAIT_RESULT,
	FLOW_OBSERVED_STEP_RUN_DISK_NEXT,
	
	FLOW_OBSERVED_STEP_RUN_EER_EXIST_CHECK,
	FLOW_OBSERVED_STEP_RUN_EER_CYL_LIFT_UP,
	FLOW_OBSERVED_STEP_RUN_EER_CYL_LIFT_DOWN,
}PLC_FLOW_OBSERVED_STEP;

void plc_flow_observed_scan(void)
{
	uint8_t id = 0;
	
	// 断点控制
	for(id = 0;  id < CONFIG_FLOW_OBSERVED_NODE_BUF_SIZE; id++){
		if(g_plc_flow_observed_port.node_buf[id].stat == TRUE)
			return;
	}
	
	// 动作流程
	switch(g_plc_flow_observed_port.step)
	{
//###############################开机初始化		
		case FLOW_OBSERVED_STEP_RST_OUT_INPUT:	//转盘初始化	
			if(plc_input_read(INPUT_MOTOR_DISK_TWO) == INPUT_STATUS_ON){/*伺服电机的one我改成了two*/
				plc_motor_set_step_pos_flow(MOTOR_ASDA_TWO, g_plc_flow_observed_port.cfg_disk_one_count/4,\
					10000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
			}

			if(plc_input_read(INPUT_MOTOR_DISK_TWO) == INPUT_STATUS_ON){
				plc_motor_set_step_pos_flow(MOTOR_ASDA_TWO, g_plc_flow_observed_port.cfg_disk_two_count/4,\
					10000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
			}	
		break;
			
		case FLOW_OBSERVED_STEP_RST_IN_INPUT:/*one改为two*/
			plc_motor_set_step_rst_flow(MOTOR_ASDA_TWO, 0,\
				10000, 500, 0.4,\
				INPUT_MOTOR_DISK_ONE, INPUT_STATUS_ON, g_plc_flow_observed_port.cfg_disk_one_offset, 1,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
		
			plc_motor_set_step_rst_flow(MOTOR_ASDA_TWO, 0,\
				10000, 500, 0.4,\
				INPUT_MOTOR_DISK_TWO, INPUT_STATUS_ON, g_plc_flow_observed_port.cfg_disk_two_offset, 1,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
		break;
		
		case FLOW_OBSERVED_STEP_RST_CLR_NG_CYL_BACK:	//收集2号转盘，滞留球
			plc_switch_set_flow(SWITCH_CYL_NG, SWITCH_STATUS_ON,\
				0, 0, 0, 1,\
				INPUT_CYL_NG_BACK, INPUT_STATUS_ON,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);

		//				plc_delay_flow(DELAY_FLOW_OBSERVED, 1000,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);

		break;
		
		case FLOW_OBSERVED_STEP_RST_CLR_NG_CYL_FRONT:
			plc_switch_set_flow(SWITCH_CYL_NG, SWITCH_STATUS_OFF,\
				0, 0, 0, 1,\
				INPUT_CYL_NG_FRONT, INPUT_STATUS_ON,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
		break;
		
		case FLOW_OBSERVED_STEP_RST_CLR_NG_MOTOR_MOVE:
			(*g_plc_flow_observed_port.p_disk_two_hold_id)++;
			
			if(*g_plc_flow_observed_port.p_disk_two_hold_id < CONFIG_FLOW_OBSERVED_TWO_HOLE_COUNT){
				plc_motor_set_step_pos_flow(MOTOR_ASDA_TWO, plc_motor_cur_postion(MOTOR_ASDA_TWO) + g_plc_flow_observed_port.cfg_disk_two_grid,\
					20000, 15000, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
				
				g_plc_flow_observed_port.step = FLOW_OBSERVED_STEP_RST_CLR_NG_CYL_BACK;
				return;
			}else{
				plc_motor_set_step_rst_flow(MOTOR_ASDA_TWO, 0,\
					20000, 15000, 0.4,\
					INPUT_MOTOR_DISK_TWO, INPUT_STATUS_ON, g_plc_flow_observed_port.cfg_disk_two_offset, 1,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
				
				// 启动搓捻电机
				plc_motor_set_bldc_flow(MOTOR_BLDC_TWIST, 2000, MOTOR_BLDC_DIR_P, g_plc_flow_observed_port.cfg_motor_bldc_twist_speed);
								
				*g_plc_flow_observed_port.p_disk_two_hold_id = 0;
				*g_plc_flow_observed_port.p_disk_one_hold_id = 0;
			}
		break;
			
//###############################轮询检测
		
		case FLOW_OBSERVED_STEP_RUN_OBJ_CLASS:
			// 设置检测标志位
			g_plc_flow_observed_port.p_camera_state->cam_one_result = g_plc_flow_observed_port.p_disk_one_hole_state[((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_CAM_ONE_OFFSET) & CONFIG_FLOW_OBSERVED_ONE_HOLE_MASK];
			g_plc_flow_observed_port.p_camera_state->cam_two_result = g_plc_flow_observed_port.p_disk_two_hole_state[((*g_plc_flow_observed_port.p_disk_two_hold_id) + CONFIG_FLOW_OBSERVED_CAM_TWO_OFFSET) & CONFIG_FLOW_OBSERVED_TWO_HOLE_MASK];
			g_plc_flow_observed_port.p_camera_state->cam_thr_result = g_plc_flow_observed_port.p_disk_two_hole_state[((*g_plc_flow_observed_port.p_disk_two_hold_id) + CONFIG_FLOW_OBSERVED_CAM_THR_OFFSET) & CONFIG_FLOW_OBSERVED_TWO_HOLE_MASK];
			g_plc_flow_observed_port.p_camera_state->cam_state      = CAM_STATE_READY;
		
			// 2号 转盘
			// 分拣	；2号转盘，次品通道位置，球的状态
			if(g_plc_flow_observed_port.p_disk_two_hole_state[((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_TWO_NG_OFFSET) & CONFIG_FLOW_OBSERVED_TWO_HOLE_MASK] == BALL_STATE_NG){
				plc_switch_set_flow(SWITCH_CYL_NG, SWITCH_STATUS_ON,\
					0, 500, 0, 1,\
					INPUT_CYL_NG_BACK, INPUT_STATUS_ON,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
				
				g_plc_flow_observed_port.p_disk_two_hole_state[((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_TWO_NG_OFFSET) & CONFIG_FLOW_OBSERVED_TWO_HOLE_MASK] = HOLE_STATE_FREE;
			}
			// 分拣	；2号转盘，合格品通道位置，球的状态
			if(g_plc_flow_observed_port.p_disk_two_hole_state[((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_TWO_OK_OFFSET) & CONFIG_FLOW_OBSERVED_TWO_HOLE_MASK] == BALL_STATE_OK){
				plc_switch_set_flow(SWITCH_CYL_OK, SWITCH_STATUS_ON,\
					0, 500, 0, 1,\
					INPUT_CYL_OK_BACK, INPUT_STATUS_ON,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
				
				g_plc_flow_observed_port.p_disk_two_hole_state[((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_TWO_OK_OFFSET) & CONFIG_FLOW_OBSERVED_TWO_HOLE_MASK] = HOLE_STATE_FREE;
			}
			
			/* 1 号转盘*/
			// 分拣	；1号转盘，次品通道位置，球的状态
			if(g_plc_flow_observed_port.p_disk_one_hole_state[((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_ONE_NG_OFFSET) & CONFIG_FLOW_OBSERVED_ONE_HOLE_MASK] == BALL_STATE_NG){	
				plc_switch_set_flow(((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_ONE_NG_OFFSET) & CONFIG_FLOW_OBSERVED_ONE_HOLE_MASK, SWITCH_STATUS_OFF,\
					0, 0, 0, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FREE, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
				
				g_plc_flow_observed_port.p_disk_one_hole_state[((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_ONE_NG_OFFSET) & CONFIG_FLOW_OBSERVED_ONE_HOLE_MASK] = HOLE_STATE_FREE;
			}	
			
			// 进料；一号转盘放球至二号转盘
			if(g_plc_flow_observed_port.p_disk_one_hole_state[((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_ONE_OK_OFFSET) & CONFIG_FLOW_OBSERVED_ONE_HOLE_MASK] == BALL_STATE_OK){
				plc_switch_set_flow(SWITCH_TRANS_AIR, SWITCH_STATUS_ON,\
					0, 900, 0, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FREE, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
				
				plc_switch_set_flow(((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_ONE_OK_OFFSET) & CONFIG_FLOW_OBSERVED_ONE_HOLE_MASK, SWITCH_STATUS_OFF,\
					350, 0, 0, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FREE, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);

				plc_switch_set_flow(SWITCH_TRANS_CYL, SWITCH_STATUS_ON,\
					400, 600, 0, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);

				
				g_plc_flow_observed_port.p_disk_one_hole_state[((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_ONE_OK_OFFSET) & CONFIG_FLOW_OBSERVED_ONE_HOLE_MASK] = HOLE_STATE_FREE;
				g_plc_flow_observed_port.p_disk_two_hole_state[*g_plc_flow_observed_port.p_disk_two_hold_id] =  HOLE_STATE_EXIST;
			}
			
			// 延时（防止无剔除和上料情况下的快速旋转）
			plc_delay_flow(DELAY_FLOW_OBSERVED, 200,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
			
			
			// 进料；抬举气缸抬料至一号转盘）
			if(plc_input_read_flow(INPUT_CYL_LIFT_EXIST) == INPUT_STATUS_ON || g_plc_flow_global_port.ball_count >= CONFIG_FLOW_GLOBAL_PORT_BALL_MAX/* f*/){			
				plc_switch_set_flow(SWITCH_CYL_LIFT, SWITCH_STATUS_ON,\
					0, 0, 0, 1,\
					INPUT_CYL_LIFT_UP, INPUT_STATUS_ON,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);	//抬举气缸，上抬
				
				plc_switch_set_flow((*g_plc_flow_observed_port.p_disk_one_hold_id), SWITCH_STATUS_ON,\
					0, 0, 0, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);	//打开响应通道气路
			}else{
				if(g_plc_flow_global_port.ball_count >= CONFIG_FLOW_GLOBAL_PORT_BALL_MAX){
					g_plc_flow_observed_port.var_lift_err_irq_mark  = plc_input_count_two(INPUT_CYL_LIFT_EXIST);
					g_plc_flow_observed_port.var_lift_err_act_count = 0;
					g_plc_flow_observed_port.step                   = FLOW_OBSERVED_STEP_RUN_EER_EXIST_CHECK;
				}else{
					g_plc_flow_observed_port.step = FLOW_OBSERVED_STEP_RUN_WAIT_RESULT;
				}
				return;
			}
		break;
		
		case FLOW_OBSERVED_STEP_RUN_CYL_LIFT_UP_DELAY:
			plc_delay_flow(DELAY_FLOW_OBSERVED, g_plc_flow_observed_port.cfg_cyl_lift_up_delay,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
		break;
		
		case FLOW_OBSERVED_STEP_RUN_CYL_LIFT_DOWN:
			plc_switch_set_flow(SWITCH_CYL_LIFT, SWITCH_STATUS_OFF,\
					0, 0, 0, 1,\
					INPUT_CYL_LIFT_DOWN, INPUT_STATUS_ON,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
		break;
		
		case FLOW_OBSERVED_STEP_RUN_CYL_LIFT_DOWN_DELAY:
			plc_delay_flow(DELAY_FLOW_OBSERVED, g_plc_flow_observed_port.cfg_cyl_lift_down_delay/2,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
		break;
			
		case FLOW_OBSERVED_STEP_RUN_CYL_LIFT_DOWN_CHECK:
			
			if(plc_input_read_flow(INPUT_CYL_LIFT_EXIST) == INPUT_STATUS_OFF){	//检测球被正确吸走
				g_plc_flow_observed_port.p_disk_one_hole_state[*g_plc_flow_observed_port.p_disk_one_hold_id] = HOLE_STATE_EXIST;
			
				plc_delay_flow(DELAY_FLOW_OBSERVED, g_plc_flow_observed_port.cfg_cyl_lift_down_delay/2,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
				
				if(g_plc_flow_global_port.ball_count > 0)	//球自计数
					g_plc_flow_global_port.ball_count--;
			}else{	//关闭响应通道气路
				plc_switch_set_flow(*g_plc_flow_observed_port.p_disk_one_hold_id, SWITCH_STATUS_OFF,\
					0, 0, 0, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
			}
		break;
			
		
		case FLOW_OBSERVED_STEP_RUN_WAIT_RESULT:
			// 等待结果
			if(g_plc_flow_observed_port.p_camera_state->cam_state == CAM_STATE_READY)
				return;
			
			// 结果录入
			g_plc_flow_observed_port.p_disk_one_hole_state[((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_CAM_ONE_OFFSET) & CONFIG_FLOW_OBSERVED_ONE_HOLE_MASK] = g_plc_flow_observed_port.p_camera_state->cam_one_result;
			g_plc_flow_observed_port.p_disk_two_hole_state[((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_CAM_TWO_OFFSET) & CONFIG_FLOW_OBSERVED_TWO_HOLE_MASK] = g_plc_flow_observed_port.p_camera_state->cam_two_result;
			g_plc_flow_observed_port.p_disk_two_hole_state[((*g_plc_flow_observed_port.p_disk_one_hold_id) + CONFIG_FLOW_OBSERVED_CAM_THR_OFFSET) & CONFIG_FLOW_OBSERVED_TWO_HOLE_MASK] = g_plc_flow_observed_port.p_camera_state->cam_thr_result;
		break;
			
		case FLOW_OBSERVED_STEP_RUN_DISK_NEXT:
			if((*g_plc_flow_observed_port.p_disk_one_hold_id) == CONFIG_FLOW_OBSERVED_ONE_HOLE_MASK){/*伺服电机的one我改成了two*/
				plc_motor_set_step_rst_flow(MOTOR_ASDA_TWO, 0,\
				7500, 500, 0.4,\
				INPUT_MOTOR_DISK_ONE, INPUT_STATUS_ON, g_plc_flow_observed_port.cfg_disk_one_offset, 1,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);		
			}else{
				plc_motor_set_step_pos_flow(MOTOR_ASDA_TWO, plc_motor_cur_postion(MOTOR_ASDA_ONE) + g_plc_flow_observed_port.cfg_disk_one_grid,\
					7500, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
			}
			
			if((*g_plc_flow_observed_port.p_disk_one_hold_id) == CONFIG_FLOW_OBSERVED_ONE_HOLE_MASK){
				plc_motor_set_step_rst_flow(MOTOR_ASDA_TWO, 0,\
					20000, 15000, 0.4,\
					INPUT_MOTOR_DISK_TWO, INPUT_STATUS_ON, g_plc_flow_observed_port.cfg_disk_two_offset, 1,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
			}else{
				plc_motor_set_step_pos_flow(MOTOR_ASDA_TWO, plc_motor_cur_postion(MOTOR_ASDA_TWO) + g_plc_flow_observed_port.cfg_disk_two_grid,\
					20000, 15000, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
			}
			(*g_plc_flow_observed_port.p_disk_one_hold_id) = ((*g_plc_flow_observed_port.p_disk_one_hold_id)  + 1) & CONFIG_FLOW_OBSERVED_ONE_HOLE_MASK;
			(*g_plc_flow_observed_port.p_disk_two_hold_id) = ((*g_plc_flow_observed_port.p_disk_two_hold_id)  + 1) & CONFIG_FLOW_OBSERVED_TWO_HOLE_MASK;
			
			g_plc_flow_observed_port.step = FLOW_OBSERVED_STEP_RUN_OBJ_CLASS;
		return;
			
//###############################传感器异常
		case FLOW_OBSERVED_STEP_RUN_EER_EXIST_CHECK:
			if(plc_input_read_flow(INPUT_CYL_LIFT_EXIST) == INPUT_STATUS_ON || g_plc_flow_observed_port.var_lift_err_irq_mark != plc_input_count_two(INPUT_CYL_LIFT_EXIST)){
				plc_switch_set_flow(SWITCH_CYL_LIFT, SWITCH_STATUS_ON,\
					0, 0, 0, 1,\
					INPUT_CYL_LIFT_UP, INPUT_STATUS_ON,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);	//抬举气缸，上抬
				
				plc_switch_set_flow((*g_plc_flow_observed_port.p_disk_one_hold_id), SWITCH_STATUS_ON,\
					0, 0, 0, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);	//打开响应通道气路
				
				g_plc_flow_observed_port.step = FLOW_OBSERVED_STEP_RUN_CYL_LIFT_UP_DELAY;
				
				return;
			}
			
			if(++(g_plc_flow_observed_port.var_lift_err_act_count) >= 3){
				g_plc_flow_global_port.ball_count = 0;
				g_plc_flow_observed_port.step     = FLOW_OBSERVED_STEP_RUN_WAIT_RESULT;
				return;
			}
		break;
		
		case FLOW_OBSERVED_STEP_RUN_EER_CYL_LIFT_UP:
			plc_switch_set_flow(SWITCH_CYL_LIFT, SWITCH_STATUS_ON,\
				0, 0, 0, 1,\
				INPUT_CYL_LIFT_UP, INPUT_STATUS_ON,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);	//抬举气缸，上抬
		break;
		
		case FLOW_OBSERVED_STEP_RUN_EER_CYL_LIFT_DOWN:
			plc_switch_set_flow(SWITCH_CYL_LIFT, SWITCH_STATUS_OFF,\
				0, 0, 0, 1,\
				INPUT_CYL_LIFT_DOWN, INPUT_STATUS_ON,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_observed_port.node_buf, &g_plc_flow_observed_port.node_pos, CONFIG_FLOW_OBSERVED_NODE_BUF_MASK);
		
			g_plc_flow_observed_port.step = FLOW_OBSERVED_STEP_RUN_EER_EXIST_CHECK;
		return;
	}
	
	g_plc_flow_observed_port.step++;
}


/*
*************************************************************************************
* 进料流程（排序）
*************************************************************************************
*/
#define CONFIG_FLOW_SORT_NODE_BUF_SIZE		8
#define CONFIG_FLOW_SORT_NODE_BUF_MASK		(CONFIG_FLOW_SORT_NODE_BUF_SIZE - 1)

#define CONFIG_FLO_SORT_AUTO_CHECK_TIME		100
#define CONFIG_FLO_SORT_AUTO_CHECK_COUNT		2
typedef struct
{
	// 运行参数
	MB_VAR_MASTER_BUF   *p_presure_sensor_buf;
	
	uint8_t             t_presuer_rxd_buf[4];
	
	uint16_t            *p_presure_sensor_get;
	uint32_t            *p_presure_sensor_total_count;
	uint32_t            *p_presure_sensor_ng_count;
	int32_t            	*p_presure_sensor_value;
	
	int32_t              presure_sensor_offset_value;	//偏移值
	
	uint8_t  						step;
	PLC_TYPE_FLOW_INFO 	node_buf[CONFIG_FLOW_SORT_NODE_BUF_SIZE];
	uint8_t 					 	node_pos;
	
	// 变量
	uint8_t     lift_exist_step;
	uint32_t    lift_exist_time_mark;	// 用于判断抬举料仓空状态
	
	uint16_t    var_flip_ball_exist_time;	//球入槽计数
	uint8_t     var_flip_ball_exist_count;
	
	// 配置参数
	uint16_t 		cfg_motor_bldc_mix_speed;
	uint16_t 		cfg_motor_bldc_trans_speed;
	
	int32_t    cfg_presure_threashold;
	
	// 料斗配置
	uint16_t    cfg_motor_filp_pos_left;
	uint16_t    cfg_motor_filp_pos_up;
	uint16_t    cfg_motor_filp_pos_right;
	uint16_t    cfg_motor_filp_pos_down;
	
}PLC_FLOW_SORT_PORT;

PLC_FLOW_SORT_PORT g_plc_flow_sort_port;

typedef enum	//流程步骤
{	
	FLOW_SORT_STEP_RST_MOTOR_TRANS_BACK = 0,
	FLOW_SORT_STEP_RST_MOTOR_FILP_OUT_INPUT,
	FLOW_SORT_STEP_RST_MOTOR_FILP_IN_INPUT,
	FLOW_SORT_STEP_RST_MOTOR_FILP_CLR,
			
	FLOW_SORT_STEP_RUN_GET_FILP_RIGHT,
	FLOW_SORT_STEP_RUN_GET_MIX_TRANS,
	FLOW_SORT_STEP_RUN_GET_WAIT_BALL,
	FLOW_SORT_STEP_RUN_GET_FILP_UP,
	FLOW_SORT_STEP_RUN_GET_FILP_UP_CHECK,
	
	FLOW_SORT_STEP_RUN_PRESSURE_OFFSET_CMD,
	FLOW_SORT_STEP_RUN_PRESSURE_OFFSET_RESULT,
	
	FLOW_SORT_STEP_RUN_PRESSURE_CYL_DOWN,
	FLOW_SORT_STEP_RUN_PRESSURE_CYL_DOWN_DELAY,
	FLOW_SORT_STEP_RUN_PRESSURE_SEND_CMD,
	FLOW_SORT_STEP_RUN_PRESSURE_RESULT,
	FLOW_SORT_STEP_RUN_PRESSURE_CYL_UP,
	FLOW_SORT_STEP_RUN_PRESSURE_ADJUST,
	
	FLOW_SORT_STEP_RUN_OUT_LIFT,
	FLOW_SORT_STEP_RUN_OUT_DELAY,
			
}PLC_FLOW_SORT_STEP;


typedef enum 
{
	PLC_FLOW_SORT_LIFT_EXIST_SCAN = 0,
	PLC_FLOW_SORT_LIFT_EXIST_WAIT,
	
}PLC_FLOW_SORT_LIFT_EXIST;


////void plc_flow_sort_scan(void)
////{
////	uint8_t id = 0;
////	
////	// 断点控制
////	for(id = 0;  id < CONFIG_FLOW_SORT_NODE_BUF_SIZE; id++){
////		if(g_plc_flow_sort_port.node_buf[id].stat == TRUE)
////			return;
////	}
////	
////	// 动作流程
////	switch(g_plc_flow_sort_port.step)
////	{
//////###############################开机初始化
////		//料斗电机初始化
////		case  FLOW_SORT_STEP_RST_MOTOR_TRANS_BACK:
////			plc_motor_set_bldc_flow(MOTOR_BLDC_TRANS, 2000, MOTOR_BLDC_DIR_N, g_plc_flow_sort_port.cfg_motor_bldc_trans_speed);
////			
////			plc_delay_flow(DELAY_FLOW_SORT, 1000,\
////				CONFIG_PLC_FEEDBACK_FLOW, 0,\
////				g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////		break;
////		
////		case FLOW_SORT_STEP_RST_MOTOR_FILP_OUT_INPUT:
////			plc_motor_set_bldc_flow(MOTOR_BLDC_TRANS, 2000, MOTOR_BLDC_DIR_N, 0);
////		
////			if(plc_input_read_flow(INPUT_MOTOR_FILP_RESET) == INPUT_STATUS_ON){
////				plc_motor_set_step_pos_flow(MOTOR_STEP_FILP, 800,\
////					2000, 500, 0.1,\
////					CONFIG_PLC_FEEDBACK_FLOW, 0,\
////					g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////			}	
////		break;
////			
////		case FLOW_SORT_STEP_RST_MOTOR_FILP_IN_INPUT:
////			plc_motor_set_step_rst_flow(MOTOR_STEP_FILP, 0,\
////				2000, 500, 0.1,\
////				INPUT_MOTOR_FILP_RESET, INPUT_STATUS_ON, g_plc_flow_sort_port.cfg_motor_filp_pos_left, -1,\
////				CONFIG_PLC_FEEDBACK_FLOW, 0,\
////				g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////		break;
////		
////		case FLOW_SORT_STEP_RST_MOTOR_FILP_CLR:
////			plc_motor_set_step_pos_flow(MOTOR_STEP_FILP, g_plc_flow_sort_port.cfg_motor_filp_pos_down,\
////					2000, 500, 0.1,\
////					CONFIG_PLC_FEEDBACK_FLOW, 0,\
////					g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////		
////			plc_motor_set_bldc_flow(MOTOR_BLDC_MIX, 2000, MOTOR_BLDC_DIR_P, g_plc_flow_sort_port.cfg_motor_bldc_mix_speed);
////		break;
////		
//////###############################运行逻辑
////		case FLOW_SORT_STEP_RUN_GET_FILP_RIGHT:	//空状态
////			g_plc_flow_sort_port.var_flip_ball_exist_time = 0;
////			
////			plc_motor_set_bldc_flow(MOTOR_BLDC_TRANS, 2000, MOTOR_BLDC_DIR_P, g_plc_flow_sort_port.cfg_motor_bldc_trans_speed/2);
////		
////			plc_motor_set_step_pos_flow(MOTOR_STEP_FILP, g_plc_flow_sort_port.cfg_motor_filp_pos_right,\
////				4000, 1000, 0.2,\
////				CONFIG_PLC_FEEDBACK_FLOW, 0,\
////				g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////		break;
////		
////		case FLOW_SORT_STEP_RUN_GET_MIX_TRANS:
////			plc_motor_set_bldc_flow(MOTOR_BLDC_TRANS, 2000, MOTOR_BLDC_DIR_P, g_plc_flow_sort_port.cfg_motor_bldc_trans_speed);
////			
////		break;
////		
////		case FLOW_SORT_STEP_RUN_GET_WAIT_BALL:
////			if(plc_input_read_flow(INPUT_MOTOR_FILP_EXIST) == INPUT_STATUS_ON){
////				g_plc_flow_sort_port.var_flip_ball_exist_count++;
////			}
////			
////			if(g_plc_flow_sort_port.var_flip_ball_exist_count >= CONFIG_FLO_SORT_AUTO_CHECK_COUNT){
////				plc_delay_flow(DELAY_FLOW_SORT, 500,\
////					CONFIG_PLC_FEEDBACK_FLOW, 0,\
////					g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////			
////				plc_motor_set_bldc_flow(MOTOR_BLDC_TRANS, 2000, MOTOR_BLDC_DIR_N, g_plc_flow_sort_port.cfg_motor_bldc_trans_speed/2);
////				
////				g_plc_flow_sort_port.var_flip_ball_exist_count = 0;
////				break;
////			}
////			
////			if(++(g_plc_flow_sort_port.var_flip_ball_exist_time) >= CONFIG_FLO_SORT_AUTO_CHECK_TIME){	// 10秒内入无球进入触发，自认为传感器可能失灵，防止死机
////				plc_motor_set_bldc_flow(MOTOR_BLDC_TRANS, 2000, MOTOR_BLDC_DIR_N, g_plc_flow_sort_port.cfg_motor_bldc_trans_speed/2);
////				break;
////			}
////		
////			plc_delay_flow(DELAY_FLOW_SORT, 100,\
////				CONFIG_PLC_FEEDBACK_FLOW, 0,\
////				g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////		return;
////			
////		case FLOW_SORT_STEP_RUN_GET_FILP_UP:			
////			plc_motor_set_step_pos_flow(MOTOR_STEP_FILP, g_plc_flow_sort_port.cfg_motor_filp_pos_up,\
////				2000, 500, 0.2,\
////				CONFIG_PLC_FEEDBACK_FREE, 0,\
////			g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////		break;
////		
////		case FLOW_SORT_STEP_RUN_GET_FILP_UP_CHECK:
////			if(plc_input_read_flow(INPUT_MOTOR_FILP_ERROR) == INPUT_STATUS_ON){
////				plc_motor_timer_deletc(MOTOR_STEP_FILP);
////			
////				g_plc_flow_sort_port.step = FLOW_SORT_STEP_RUN_GET_FILP_RIGHT;
////				return;
////			}
////			if(plc_motor_cur_postion(MOTOR_STEP_FILP) == g_plc_flow_sort_port.cfg_motor_filp_pos_up){
////				break;
////			}
////		return;
////			
////		case FLOW_SORT_STEP_RUN_PRESSURE_OFFSET_CMD:	//修正值 - 命令
////			g_plc_flow_sort_port.p_presure_sensor_buf = mb_master_read(CONFIG_MB_PORT_USART2, 3, 0, 2, g_plc_flow_sort_port.t_presuer_rxd_buf, 200);
////		break;
////		
////		case FLOW_SORT_STEP_RUN_PRESSURE_OFFSET_RESULT:
////			if(g_plc_flow_sort_port.p_presure_sensor_buf->pos != CONFIG_MASTER_POS_RESULT){			//修正值 - 等待结果
////				return;
////			}	
////			g_plc_flow_sort_port.presure_sensor_offset_value = (((int32_t)g_plc_flow_sort_port.t_presuer_rxd_buf[0])<<24) + (((int32_t)g_plc_flow_sort_port.t_presuer_rxd_buf[1])<<16) +\
////				(((int32_t)g_plc_flow_sort_port.t_presuer_rxd_buf[2])<<8) + ((int32_t)g_plc_flow_sort_port.t_presuer_rxd_buf[3]);

////			g_plc_flow_sort_port.p_presure_sensor_buf->pos = CONFIG_MASTER_POS_FREE;
////		break;
////			
////		case FLOW_SORT_STEP_RUN_PRESSURE_CYL_DOWN:
////			plc_motor_set_bldc_flow(MOTOR_BLDC_TRANS, 2000, MOTOR_BLDC_DIR_P, 0);
////		
////			plc_switch_set_flow(SWITCH_CYL_PRESSURE, SWITCH_STATUS_ON,\
////				0, 0, 0, 1,\
////				-1, INPUT_STATUS_ON,\
////				CONFIG_PLC_FEEDBACK_FREE, 0,\
////				g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////		break;
////		
////		case FLOW_SORT_STEP_RUN_PRESSURE_CYL_DOWN_DELAY:
////			plc_delay_flow(DELAY_FLOW_SORT, 600,\
////				CONFIG_PLC_FEEDBACK_FLOW, 0,\
////				g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////		break;
////		
////		case FLOW_SORT_STEP_RUN_PRESSURE_SEND_CMD:
////			 g_plc_flow_sort_port.p_presure_sensor_buf = mb_master_read(CONFIG_MB_PORT_USART2, 3, 0, 2, g_plc_flow_sort_port.t_presuer_rxd_buf, 200);
////		
////			if(g_plc_flow_sort_port.var_flip_ball_exist_time < CONFIG_FLO_SORT_AUTO_CHECK_TIME){
////				(*g_plc_flow_sort_port.p_presure_sensor_total_count)++;	//个数计数
////			}
////		break;
////		
////		case FLOW_SORT_STEP_RUN_PRESSURE_RESULT:
////			if(g_plc_flow_sort_port.p_presure_sensor_buf->pos != CONFIG_MASTER_POS_RESULT){			//等待结果
////				return;
////			}
////			
////			*g_plc_flow_sort_port.p_presure_sensor_value = (((int32_t)g_plc_flow_sort_port.t_presuer_rxd_buf[0])<<24) + (((int32_t)g_plc_flow_sort_port.t_presuer_rxd_buf[1])<<16) +\
////				(((int32_t)g_plc_flow_sort_port.t_presuer_rxd_buf[2])<<8) + ((int32_t)g_plc_flow_sort_port.t_presuer_rxd_buf[3]);

////			*g_plc_flow_sort_port.p_presure_sensor_value -= g_plc_flow_sort_port.presure_sensor_offset_value;
////			
////			g_plc_flow_sort_port.p_presure_sensor_buf->pos = CONFIG_MASTER_POS_FREE;
////		break;
////		
////		case FLOW_SORT_STEP_RUN_PRESSURE_CYL_UP:
////			plc_switch_set_flow(SWITCH_CYL_PRESSURE, SWITCH_STATUS_OFF,\
////				0, 0, 0, 1,\
////				-1, 0,\
////				CONFIG_PLC_FEEDBACK_FREE, 0,\
////				g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////		
////			plc_delay_flow(DELAY_FLOW_SORT, 50,\
////				CONFIG_PLC_FEEDBACK_FLOW, 0,\
////				g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////		break;
////		
////		case FLOW_SORT_STEP_RUN_PRESSURE_ADJUST:			
////			if(*g_plc_flow_sort_port.p_presure_sensor_value < g_plc_flow_sort_port.cfg_presure_threashold){
////				plc_motor_set_step_pos_flow(MOTOR_STEP_FILP, g_plc_flow_sort_port.cfg_motor_filp_pos_down,\
////					3000, 1000, 0.2,\
////					CONFIG_PLC_FEEDBACK_FLOW, 0,\
////					g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////				
////				if(*g_plc_flow_sort_port.p_presure_sensor_value > 25){
////					(*g_plc_flow_sort_port.p_presure_sensor_ng_count)++;	//不合格计数
////				}
////				g_plc_flow_sort_port.step = FLOW_SORT_STEP_RUN_GET_FILP_RIGHT;
////				return;
////			}		
////		break;
////			
////		case FLOW_SORT_STEP_RUN_OUT_LIFT:
////			// 判断是否下球
////			if((plc_input_read_flow(INPUT_CYL_LIFT_EXIST) == INPUT_STATUS_ON && g_plc_flow_global_port.ball_count > CONFIG_FLOW_GLOBAL_PORT_BALL_CNT) ||\
////				g_plc_flow_global_port.ball_count >= CONFIG_FLOW_GLOBAL_PORT_BALL_MAX)
////				return;
////			
////			plc_motor_set_step_rst_flow(MOTOR_STEP_FILP, 0,\
////					3000, 1000, 0.2,\
////					INPUT_MOTOR_FILP_RESET, INPUT_STATUS_ON, g_plc_flow_sort_port.cfg_motor_filp_pos_left, -1,\
////					CONFIG_PLC_FEEDBACK_FLOW, 0,\
////					g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////		break;
////		
////		case FLOW_SORT_STEP_RUN_OUT_DELAY:				
////			g_plc_flow_global_port.ball_count++;	//球个数自加
////			
////			plc_delay_flow(DELAY_FLOW_SORT, 200,\
////				CONFIG_PLC_FEEDBACK_FLOW, 0,\
////				g_plc_flow_sort_port.node_buf, &g_plc_flow_sort_port.node_pos, CONFIG_FLOW_SORT_NODE_BUF_MASK);
////		
////			g_plc_flow_sort_port.step = FLOW_SORT_STEP_RUN_GET_FILP_RIGHT;
////		return; 
////	}
////	
////	g_plc_flow_sort_port.step++;
////}

/////*
////*************************************************************************************
////* 拉力传感器
////*************************************************************************************
////*/
////void plc_pull_sensor()
////{
////	if(*g_plc_flow_sort_port.p_presure_sensor_get == 0){
////		return;
////	}else if(*g_plc_flow_sort_port.p_presure_sensor_get == 1)	// 发送指令
////	{
////		g_plc_flow_sort_port.p_presure_sensor_buf = mb_master_read(CONFIG_MB_PORT_USART2, 3, 0, 2, g_plc_flow_sort_port.t_presuer_rxd_buf, 500);
////		
////		*g_plc_flow_sort_port.p_presure_sensor_get = 2;
////	}else if(*g_plc_flow_sort_port.p_presure_sensor_get == 2)	// 等待结果
////	{
////		if(g_plc_flow_sort_port.p_presure_sensor_buf->pos != CONFIG_MASTER_POS_RESULT){			
////			return;
////		}else{
////			*g_plc_flow_sort_port.p_presure_sensor_value = (((uint32_t)g_plc_flow_sort_port.t_presuer_rxd_buf[0])<<24) + (((uint32_t)g_plc_flow_sort_port.t_presuer_rxd_buf[1])<<16) +\
////				(((uint32_t)g_plc_flow_sort_port.t_presuer_rxd_buf[2])<<8) + ((uint32_t)g_plc_flow_sort_port.t_presuer_rxd_buf[3]);
////			
////			 g_plc_flow_sort_port.p_presure_sensor_buf->pos = CONFIG_MASTER_POS_FREE;
////			
////			*g_plc_flow_sort_port.p_presure_sensor_get = 0;	//释放
////		}
////	}
////}


/////*
////*************************************************************************************
////* 逻辑流程
////*************************************************************************************
////*/

void plc_flow_config(void)
{
	// 上电使能
	plc_eep_read_value(0, 39*4, 4, (void *)&(g_plc_flow_observed_port.cfg_sys_poweron_enable));
	
	// 料仓配置
	plc_eep_read_value(0, 0*4, 4, (void *)&(g_plc_flow_sort_port.cfg_motor_bldc_mix_speed));
	plc_eep_read_value(0, 1*4, 4, (void *)&(g_plc_flow_sort_port.cfg_motor_bldc_trans_speed));
	plc_eep_read_value(0, 2*4, 4, (void *)&(g_plc_flow_sort_port.cfg_presure_threashold));
	
	//  料斗配置
	plc_eep_read_value(0, 4*4, 4, (void *)&(g_plc_flow_sort_port.cfg_motor_filp_pos_left));
	plc_eep_read_value(0, 5*4, 4, (void *)&(g_plc_flow_sort_port.cfg_motor_filp_pos_up));
	plc_eep_read_value(0, 6*4, 4, (void *)&(g_plc_flow_sort_port.cfg_motor_filp_pos_right));
	plc_eep_read_value(0, 7*4, 4, (void *)&(g_plc_flow_sort_port.cfg_motor_filp_pos_down));

	// 转盘配置
	plc_eep_read_value(0, 8*4,  4, (void *)&(g_plc_flow_observed_port.cfg_disk_one_offset));
	plc_eep_read_value(0, 9*4,  4, (void *)&(g_plc_flow_observed_port.cfg_disk_one_count));
	plc_eep_read_value(0, 10*4, 4, (void *)&(g_plc_flow_observed_port.cfg_disk_two_offset));
	plc_eep_read_value(0, 11*4, 4, (void *)&(g_plc_flow_observed_port.cfg_disk_two_count));
	
	g_plc_flow_observed_port.cfg_disk_one_grid = g_plc_flow_observed_port.cfg_disk_one_count/CONFIG_FLOW_OBSERVED_ONE_HOLE_COUNT;
	g_plc_flow_observed_port.cfg_disk_two_grid = g_plc_flow_observed_port.cfg_disk_two_count/CONFIG_FLOW_OBSERVED_TWO_HOLE_COUNT;
	
	// 搓捻电机配置
	plc_eep_read_value(0, 12*4,  4, (void *)&(g_plc_flow_observed_port.cfg_motor_bldc_twist_speed));
	
	// 抬举气缸
	plc_eep_read_value(0, 13*4,  4, (void *)&(g_plc_flow_observed_port.cfg_cyl_lift_up_delay));
	plc_eep_read_value(0, 14*4,  4, (void *)&(g_plc_flow_observed_port.cfg_cyl_lift_down_delay));
	
	// 变量映射（相机）
	g_plc_flow_observed_port.p_camera_state = (PLC_FLOW_OBSERVED_CAMERA *)pusRegHoldingBuf;
	g_plc_flow_observed_port.p_camera_state->cam_state = CAM_STATE_FREE;	
	
	// 变量映射（压力传感器）
//	g_plc_flow_sort_port.p_presure_sensor_total_count  = (uint32_t *)(pusRegHoldingBuf + 4);
//	g_plc_flow_sort_port.p_presure_sensor_ng_count     = (uint32_t *)(pusRegHoldingBuf + 8);
//	g_plc_flow_sort_port.p_presure_sensor_value        =  (int32_t *)(pusRegHoldingBuf + 12);
//	g_plc_flow_sort_port.p_presure_sensor_get          = (uint16_t *)(pusRegHoldingBuf + 16);
//	*g_plc_flow_sort_port.p_presure_sensor_total_count = 0;
//	*g_plc_flow_sort_port.p_presure_sensor_ng_count    = 0;
//	*g_plc_flow_sort_port.p_presure_sensor_value       = 0;
//	*g_plc_flow_sort_port.p_presure_sensor_get         = 0;
	
	// 变量映射（转盘）
//	g_plc_flow_observed_port.p_disk_one_hold_id    = (uint8_t *)(pusRegHoldingBuf + 18);
//	g_plc_flow_observed_port.p_disk_two_hold_id    = (uint8_t *)(pusRegHoldingBuf + 19);
//	g_plc_flow_observed_port.p_disk_one_hole_state = (uint8_t *)(pusRegHoldingBuf + 20);
//	g_plc_flow_observed_port.p_disk_two_hole_state = (uint8_t *)(pusRegHoldingBuf + 28);
	
	// 软件按键
	p_g_flow_action_enable  = (uint16_t *)(pusRegHoldingBuf + 40);
	*p_g_flow_action_enable = 0x01;
	
	// 变量初始化
	g_plc_flow_sort_port.lift_exist_step = PLC_FLOW_SORT_LIFT_EXIST_SCAN;
	if(plc_input_read_flow(INPUT_CYL_LIFT_EXIST) == INPUT_STATUS_ON)
		g_plc_flow_global_port.ball_count = CONFIG_FLOW_GLOBAL_PORT_BALL_CNT;
	else
		g_plc_flow_global_port.ball_count = 0;
}

////void plc_flow_scan(void)
////{
////	// 传感器（称重）
////	plc_pull_sensor();
////	
////	//暂停系统
////	if(*p_g_flow_action_enable == 0)
////		return;
////	
////	// 上电使能
////	if(g_plc_flow_observed_port.cfg_sys_poweron_enable == 0)
////		return;
////	
////	// 检测逻辑
////	plc_flow_observed_scan();
////		
////	// 进料逻辑
////	plc_flow_sort_scan();
////}
