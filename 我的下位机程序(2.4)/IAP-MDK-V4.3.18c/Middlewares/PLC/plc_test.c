#include "plc_switch.h"
#include "plc_bridge.h"
#include "plc_beep.h"
#include "plc_delay.h"
#include "plc_motor.h"
#include "plc_test.h"
#include "mb_cb.h"
#include "mb_master.h"
#include "plc_button.h"
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
	
/***************************************************************
*E5678未用
*E8暂时定位后退保护
*E5开启按键
*E6 停止按键
*E7  黄色按键，暂定复位
****************************************************************/
	
/*自定义变量*/
static uint32_t flag_one = 0;//200->6.6mm
static uint32_t flag_two = 0;//1000->1mm
static uint32_t flag_tf = 26520;//400->1mm

static int32_t flag_motor_five = 0;//570mm
static int32_t flag_motor_six = 0;//0mm
static int32_t flag_motor_seven = 0;//0mm

//static uint32_t flag_motor_astone_two = 0;//0mm
//static uint8_t start_buttom = 0;  //启动时为1，不启动时为0
static uint8_t m1position = 0;  //为1时显示频闪时M1的位置，为0时显示M1的实时位置
	


//static uint8_t finishRunFlag = 0;
	
void motor_astaone(void);
void motor_astatwo(void);
void motor_steptf(void);
void motor_step_waste(void);

void motor_step_waste_M5(void);
void motor_step_waste_M6(void);
void motor_step_waste_M7(void);
void button_con(void);
	
//void finishRun();
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

	/*伺服电机M1*/
	//motor_astaone();	

	/*伺服电机M2*/
	//motor_astatwo();
	
	/*同步电机M3，M4开始运动步进控制*/
	//motor_steptf();
	
	/*电机废品吸取放置*/
	//motor_step_waste();
	//电机运行顺序控制
	switch (pusRegHoldingBuf[34])
	{
		case 1:		//M1 M2 (M3 M4) M5 M6 M7
			
			motor_astaone();	
			
			motor_astatwo();	
			
			motor_steptf();		//同步电机M3，M4开始运动步进控制
			
			//motor_step_waste();	//电机废品吸取放置/
		
			motor_step_waste_M5();		//取次品步进电机M5开始运动
		
			motor_step_waste_M6();		//取次品步进电机M6开始运动
		
			motor_step_waste_M7();		//取次品步进电机M7开始运动
			break;
		
		case 2:		//M2 (M3 M4) M1 M5 M6 M7
			
			motor_astatwo();	//伺服电机M2//
			
			motor_steptf();		//同步电机M3，M4开始运动步进控制//
			
			motor_astaone();	//伺服电机M1//
			
			//motor_step_waste();	//电机废品吸取放置//
		
			motor_step_waste_M5();		//取次品步进电机M5开始运动
		
			motor_step_waste_M6();		//取次品步进电机M6开始运动
		
			motor_step_waste_M7();		//取次品步进电机M7开始运动
			break;
		default:
			pusRegHoldingBuf[34] = 1;
			break;
	}
		
}

  
void Close_read_camera()
{
	//M2在拍照范围内
	if(flag_two == 67000 || flag_two == 126000 || flag_two == 185000 || flag_two == 244000 || flag_two == 303000 || flag_two == 362000 )
	{
		//M34在拍照范围内
		//if(flag_tf == 26120)
		{
			pusRegHoldingBuf[30] = 99;		//完成一列拍照
		}
	}
}
	
uint8_t plc_motor_pos_flag(uint8_t flag1)
{
	if(flag1==1)
	{
		//M2在拍照范围内
		if(flag_two == 67000 || flag_two == 126000 || flag_two == 185000 || flag_two == 244000 || flag_two == 303000 || flag_two == 362000 )
		{
			//M34在拍照范围内
			//if(flag_tf == 26120)
			{
				if(pusRegHoldingBuf[36] == 0)
				{
					if(pusRegHoldingBuf[28] == 1)
						pusRegHoldingBuf[10] = 1;	//上层相机环光拍照
					else
						pusRegHoldingBuf[10] = 6;	//上层相机点光拍照
				}
				else 
				{
					if(pusRegHoldingBuf[28] == 1)
						pusRegHoldingBuf[10] = 8;	//下层相机环光拍照
					else
						pusRegHoldingBuf[10] = 9;	//下层相机点光拍照
				}
			}
			
				
		}
		
	}
	return 0;
}

void finishRst(uint8_t id)
{
	switch(id)
	{
		case 0:
			pusRegHoldingBuf[6] = 99;
			break;
		case 1:
			pusRegHoldingBuf[2] = 99;
			break;
		case 3:
			pusRegHoldingBuf[4] = 99;
			break;
		case 4:
			pusRegHoldingBuf[20] = 99;
			break;
		case 5:
			pusRegHoldingBuf[22] = 99;
			break;
		case 6:
			pusRegHoldingBuf[24] = 99;
			break;
		default:
			break;	
	}
	if(pusRegHoldingBuf[6] == 99 && pusRegHoldingBuf[2] == 99 && pusRegHoldingBuf[4] == 99 && pusRegHoldingBuf[20] == 99 && pusRegHoldingBuf[22] == 99 && pusRegHoldingBuf[24] == 99)
	{
		pusRegHoldingBuf[38] = 1;
	}
	else
	{
		//pusRegHoldingBuf[38] = 0;
	}
	return;
}

void finishRun(uint8_t id)
{
	switch(id)
	{
		case 0:
			pusRegHoldingBuf[6] = 99;
			break;
		case 1:
			pusRegHoldingBuf[2] = 99;
			break;
		case 3:
			pusRegHoldingBuf[4] = 99;
			break;
		case 4:
			pusRegHoldingBuf[20] = 99;
			break;
		case 5:
			pusRegHoldingBuf[22] = 99;
			break;
		case 6:
			pusRegHoldingBuf[24] = 99;
			break;
		default:
			break;	
	}
	return;
}

/**************************************************************************
*40006                 40001
*0  复位吸放一次       1：启动键亮绿灯
*1  相机触发			  2：停止键按红灯								
*2  放气			  3：蜂鸣器报警亮黄灯
*3  吸气			  
*4：料盘释放
****************************************************************************/
void plc_flow_test_switch_scan(void)
{
	uint8_t id = 0;
		
	// 断点控制
	for(id = 0;  id < CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_SIZE; id++)
	{
		if(g_plc_flow_test_switch_port.node_buf[id].stat == TRUE)
			return;
	}
		
	/*按钮开关*/
	button_con();
	//	case 0:		/*接口号    启动延时   间隔时间      开关几次  x   x    */
		
	int dis, dis1, dis2; //dis为电机M1当前位置，dis = dis1 * 256 + dis2
	
	if(plc_input_read_flow(15)==0 && plc_input_read_flow(0) == 0 && plc_input_read_flow(2) == 0 && plc_input_read_flow(10) == 0 && plc_input_read_flow(8) == 0 && plc_input_read_flow(12) == 0)
	{
		pusRegHoldingBuf[38] = 1;
	}
		
	switch (pusRegHoldingBuf[0])
    {
			
		//启动键亮绿灯
		case 1:
			pusRegHoldingBuf[0] = 98;
		  //pusRegHoldingBuf[10] = 0;	//吸盘开机启动
			pusRegHoldingBuf[34] = 1;	//启动键按下去电机的运行顺序，M1 M2 M3 M4 M5 M6 M7
			pusRegHoldingBuf[6] = 0;	//M1复位
			pusRegHoldingBuf[2] = 0;	//M2复位
			pusRegHoldingBuf[4] = 0;	//M3M4复位
			//pusRegHoldingBuf[12] = 0;	//M5M6M7复位
			pusRegHoldingBuf[20] = 0;	//M5复位
			pusRegHoldingBuf[22] = 0;	//M6复位
			pusRegHoldingBuf[24] = 0;	//M7复位
			  
			plc_button_run();
		//蜂鸣器不响
			plc_switch_set_flow(0,\
				0, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
		//红灯不亮
			plc_switch_set_flow(1,\
				0, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
		//黄灯不亮	
			plc_switch_set_flow(2,\
				0, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
		//绿灯亮		
			plc_switch_set_flow(3,\
				1, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			pusRegHoldingBuf[0] = 99;
			pusRegHoldingBuf[14] = 1;		//通知上位机设备开机键已按下
			break;
				
		//停止键亮红灯	
		case 2:
			pusRegHoldingBuf[0] = 98;
			plc_button_stop();
		//黄灯亮
			plc_switch_set_flow(2,\
				1,0,0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
		//蜂鸣器响
			plc_switch_set_flow(0,\
				1, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
		//红灯不亮	
			plc_switch_set_flow(1,\
				0, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
		//绿灯不亮	
			plc_switch_set_flow(3,\
				0, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);				
			break;
				
		//蜂鸣器报警亮黄灯
		case 3:
			pusRegHoldingBuf[0] = 97;
			//蜂鸣器响
			plc_switch_set_flow(0,\
				1, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			//红灯不亮	
			plc_switch_set_flow(1,\
				0,  0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			//黄灯亮	
			plc_switch_set_flow(2,\
				1, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			//绿灯不亮		
			plc_switch_set_flow(3,\
				0, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			break;
	//开机键按下亮绿灯
		case 4:
			pusRegHoldingBuf[0] = 99;
		  //pusRegHoldingBuf[10] = 0;	//吸盘开机启动
			pusRegHoldingBuf[34] = 1;	//启动键按下去电机的运行顺序，M1 M2 M3 M4 M5 M6 M7
			pusRegHoldingBuf[6] = 0;	//M1复位
			pusRegHoldingBuf[2] = 0;	//M2复位
			pusRegHoldingBuf[4] = 0;	//M3M4复位
			//pusRegHoldingBuf[12] = 0;	//M5M6M7复位
			pusRegHoldingBuf[20] = 0;	//M5复位
			pusRegHoldingBuf[22] = 0;	//M6复位
			pusRegHoldingBuf[24] = 0;	//M7复位
			  
			plc_button_run();
			//蜂鸣器不响
			plc_switch_set_flow(0,\
				0, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			//红灯不亮
			plc_switch_set_flow(1,\
				0, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			//黄灯不亮		
			plc_switch_set_flow(2,\
				0, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			//绿灯亮		
			plc_switch_set_flow(3,\
				1, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			pusRegHoldingBuf[14] = 1;		//通知上位机设备开机键已按下
			break;
			
		//软件上的继续键按下亮绿灯
		case 5:
			pusRegHoldingBuf[0] = 99;	//99电机才会跑
			//plc_button_run();
		//蜂鸣器不响
			plc_switch_set_flow(0,\
				0, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
		//黄灯不亮
			plc_switch_set_flow(2,\
				0,0,0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
		//红灯不亮	
			plc_switch_set_flow(1,\
				0, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
		//绿灯亮
			plc_switch_set_flow(3,\
				1, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);	
			break;
		default:
			break;				
	}
		
	switch(pusRegHoldingBuf[10])
	{
		//控制吸盘开机启动
		case 0:
			pusRegHoldingBuf[10] = 99;
			
			plc_switch_set_flow(8,\
				1, 0, 500, 500, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
				break;
			
		//相机与环光24V触发
		case 1:
			pusRegHoldingBuf[10] = 98;
				
			plc_switch_set_flow(11,\
				1, 0, 10, 10, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			
			plc_switch_set_flow(10,\
					1, 0, 10, 10, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			
			m1position = 1;
			dis = plc_motor_cur_postion(0);
			dis1 = dis / 256;
			dis2 = dis % 256;
			pusRegHoldingBuf[8] = dis1;
			pusRegHoldingBuf[16] = dis2;
			pusRegHoldingBuf[10] = 99;
			pusRegHoldingBuf[26] = 1;		//上位机开始读取照片
			break;
			
		//控制次品吸盘(放气)
		case 2:
			pusRegHoldingBuf[10] = 99;
			
			plc_switch_set_flow(8,\
				0, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			break;
			
		//控制次品吸盘(吸气)
		case 3:
			pusRegHoldingBuf[10] = 99;
			
			plc_switch_set_flow(8,\
				1, 0, 0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			break;
			
		//料盘释放
		case 4:
			pusRegHoldingBuf[10] = 99;
			
			plc_switch_set_flow(4,\
					1, 0, 0, 0, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			break;
			
		//料盘固定
		case 5:
			pusRegHoldingBuf[10] = 99;
			
			plc_switch_set_flow(4,\
					0, 0, 0, 0, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			break;
			
		//相机与点光触发
		case 6:
			pusRegHoldingBuf[10] = 98;

			plc_switch_set_flow(12,\
					1, 0, 10, 10, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			
			plc_switch_set_flow(10,\
					1, 0, 10, 10, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			m1position = 1;
			dis = plc_motor_cur_postion(0);
			dis1 = dis / 256;
			dis2 = dis % 256;
			pusRegHoldingBuf[8] = dis1;
			pusRegHoldingBuf[16] = dis2;
			pusRegHoldingBuf[10] = 99;
			pusRegHoldingBuf[26] = 1;		//上位机开始读取照片
			break;
			
		case 7:
			pusRegHoldingBuf[10] = 99;
			m1position = 0;
			break;
			
		//下面相机与环光24V触发
		case 8:
			pusRegHoldingBuf[10] = 98;
				
			plc_switch_set_flow(14,\
				1, 0, 10, 10, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			
			plc_switch_set_flow(13,\
					1, 0, 10, 10, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			
			m1position = 1;
			dis = plc_motor_cur_postion(0);
			dis1 = dis / 256;
			dis2 = dis % 256;
			pusRegHoldingBuf[8] = dis1;
			pusRegHoldingBuf[16] = dis2;
			pusRegHoldingBuf[10] = 99;
			pusRegHoldingBuf[26] = 1;		//上位机开始读取照片
			break;
		//下面相机与点光触发
		case 9:
			pusRegHoldingBuf[10] = 98;

			plc_switch_set_flow(15,\
					1, 0, 10, 10, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			
			plc_switch_set_flow(13,\
					1, 0, 10, 10, 1,\
					-1, 0,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);
			m1position = 1;
			dis = plc_motor_cur_postion(0);
			dis1 = dis / 256;
			dis2 = dis % 256;
			pusRegHoldingBuf[8] = dis1;
			pusRegHoldingBuf[16] = dis2;
			pusRegHoldingBuf[10] = 99;
			pusRegHoldingBuf[26] = 1;		//上位机开始读取照片
			break;
		//猫眼相机光源启动
		/*
		case 10:
			pusRegHoldingBuf[10] = 98;
			//plc_button_stop();
		//灯亮
			plc_switch_set_flow(6,\
				1,0,0, 0, 1,\
				-1, 0,\
				CONFIG_PLC_FEEDBACK_FLOW, 0,\
				g_plc_flow_test_switch_port.node_buf, &g_plc_flow_test_switch_port.node_pos, CONFIG_PLC_TEST_SWITCH_FLOW_NODE_BUF_MASK);

			break;
		*/

		default:
			break;
	}
		
	if (m1position == 0)  //为0时显示实时距离
	{
		int dis = plc_motor_cur_postion(0);
		int dis1 = dis / 256;
		int dis2 = dis % 256;
		pusRegHoldingBuf[8] = dis1;
		pusRegHoldingBuf[16] = dis2;
	}
		

}
	
void plc_flow_test_scan(void)
{
	plc_flow_test_switch_scan();
	if(pusRegHoldingBuf[0]==99)//其实用到switch会使用的更好更方便
		plc_flow_test_motor_scan();		
}
	
#endif
/***************************************************************************
*
*伺服电机1  地址40004
* 0：复位   1：后退6.6mm   2：前进6.6     5：后退1mm    6：前进1mm     (起始值，基于第一个标准位置上)
* 7：复位后前进的第1基准值，在检测时候使用
* 9：复位后前进的第二基准值，在用到吸取废品时使用
* 3：后退34.4mm       4：前进34.4mm    （起始值基于第二个标准位置上，位置还未确定）
* 9-30所加的功能，加了更多的速度复位，其中  

*****************************************************************************/
void motor_astaone()
{	
	uint8_t id = 0;
		
	// 断点控制
	for(id = 0;  id < CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_SIZE; id++)
	{
		if(g_plc_flow_test_motor_port.node_buf[id].stat == TRUE)
			return;
	}
		
	switch(pusRegHoldingBuf[6])
  {
		//M1向后复位
		case 0:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[6] = 98;
				if(plc_input_read_flow(7)!=0)
				{
					//后期再加保护最后的限位,,,测试使用E8
					plc_motor_set_step_rst_flow(0, 0,\
						500, 100, 0.5,\
						15, 0, 0, -1,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
					
					flag_one = 0;
					//pusRegHoldingBuf[6] = 10;
					//pusRegHoldingBuf[6] = 99;
				}		
			}
			
			break;
		
		//后退462毫米			
		case 1:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[6] = 98;
		
				if(plc_input_read_flow(15) != 0)
				{
					//后退限位E16
					flag_one = flag_one - 15060;//步进值
					plc_motor_set_step_pos_flow(0, flag_one,\
						2000, 500, 4,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
				}
				pusRegHoldingBuf[30] = 1;		//开始一列拍照
				pusRegHoldingBuf[6] = 99;
				
				if(pusRegHoldingBuf[36] == 0)
				{
					if(pusRegHoldingBuf[28] == 1)
						pusRegHoldingBuf[10] = 1;	//上层相机环光拍照
					else
						pusRegHoldingBuf[10] = 6;	//上层相机点光拍照
				}
				else 
				{
					if(pusRegHoldingBuf[28] == 1)
						pusRegHoldingBuf[10] = 8;	//下层相机环光拍照
					else
						pusRegHoldingBuf[10] = 9;	//下层相机点光拍照
				}
			}
			break;
		
		//前进462毫米
		case 2:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[6] = 98;	
		
				if(plc_input_read_flow(14) != 0)
				{
					//前进限位E15
					flag_one = flag_one + 15060;//步进值
					plc_motor_set_step_pos_flow(0, flag_one,\
						2000, 500, 4,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
				}		
				pusRegHoldingBuf[30] = 1;		//开始一列拍照
				pusRegHoldingBuf[6] = 99;
				
				if(pusRegHoldingBuf[36] == 0)
				{
					if(pusRegHoldingBuf[28] == 1)
						pusRegHoldingBuf[10] = 1;	//上层相机环光拍照
					else
						pusRegHoldingBuf[10] = 6;	//上层相机点光拍照
				}
				else 
				{
					if(pusRegHoldingBuf[28] == 1)
						pusRegHoldingBuf[10] = 8;	//下层相机环光拍照
					else
						pusRegHoldingBuf[10] = 9;	//下层相机点光拍照
				}
			}
			break;
			
		
		
		//第一基准值
		case 7:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[6] = 98;
			
				if(plc_input_read_flow(14) != 0)			
				{
					//前进限位E15
					plc_motor_set_step_pos_flow(0, 5550,\
					2000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
					flag_one = 5550;
				}
				
				//pusRegHoldingBuf[6] = 99;
			}
			break;
			
		//第二基准值
		case 9:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[6] = 98;
			
				if((plc_input_read_flow(14) != 0))			
				{
					//前进限位E15
					flag_one = 348;
					plc_motor_set_step_pos_flow(0, flag_one,\
					2000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
				}
				
				//pusRegHoldingBuf[6] = 99;
			}
			break;
		
		//电机运转通用
		case 20:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[6] = 98;	
		
				if(plc_input_read_flow(14) != 0)	//没到纵向导轨右侧限位开关，M1继续前进
				{
					
					//前进限位E15
					flag_one = pusRegHoldingBuf[18]*256 + pusRegHoldingBuf[32];//步进值
					plc_motor_set_step_pos_flow(0, flag_one,\
						10000, 100, 0.1,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
				}		
				
				//pusRegHoldingBuf[6] = 99;	
			}
			break;
			
		default:
			pusRegHoldingBuf[6] = 99;
			break;
			
	}
}	
	
/***************************************************************************
*
*伺服电机2  地址40002
* 0：右端复位   1：左移6mm   2：右移6mm     3：左端复位    4：左移59mm      5：右移59mm
* 对40008写步进值
*****************************************************************************/
void motor_astatwo()
{
	uint8_t id = 0;
		
	// 断点控制
	for(id = 0;  id < CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_SIZE; id++)
	{
		if(g_plc_flow_test_motor_port.node_buf[id].stat == TRUE)
			return;
	}
	
	switch (pusRegHoldingBuf[2])
	{
		case 0:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[2] = 98;
				if(plc_input_read_flow(0) != 0)
				{
					
					//限位E1
					plc_motor_set_step_rst_flow(1, 0,\
					64000, 15000, 0.4,\
					0, 0, 0, -1,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
				}
				flag_two = 0;
				//pusRegHoldingBuf[2] = 99;	//复位后平调基准值
			}
			break;
			
		//左移59毫米
		case 4:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[2] = 98;
			
				if(plc_input_read_flow(1) != 0)//E2限位
				{
					flag_two = flag_two + 59000;
					plc_motor_set_step_pos_flow(1, flag_two,\
					64000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);		
				}
				
				//pusRegHoldingBuf[2] = 99;
			}
			break;
		
		//右移59毫米
		case 5:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[2] = 98;
			
				if(plc_input_read_flow(0) != 0 && flag_two - 59000 > 0)//E1限位
				{
					flag_two = flag_two - 59000;
					
					plc_motor_set_step_pos_flow(1, flag_two,\
					64000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
						
				}
			
				//pusRegHoldingBuf[2] = 99;
			}
			break;
		
		//左移295mm
		case 6:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[2] = 98;
			
				if(plc_input_read_flow(1) != 0 && flag_two == 0)//E1限位
				{
					flag_two = flag_two + 295000;
					
					plc_motor_set_step_pos_flow(1, flag_two,\
					64000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
						
				}
				
				//pusRegHoldingBuf[2] = 99;
			}
			break;
		
		//右移295mm
		case 7:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[2] = 98;
			
				if(plc_input_read_flow(0) != 0 && flag_two == 295000)//E1限位
				{
					flag_two = flag_two - 295000;
					
					plc_motor_set_step_pos_flow(1, flag_two,\
					64000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
						
				}
				
				//pusRegHoldingBuf[2] = 99;
			}
			break;
			
		
		//通用上位机控制
		case 20:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[2] = 98;
			
				if(plc_input_read_flow(1) != 0)//E1限位
				{
					flag_two = (pusRegHoldingBuf[18]*256 + pusRegHoldingBuf[32])*10;
					
					plc_motor_set_step_pos_flow(1, flag_two,\
					25000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
						
				}
				
				//pusRegHoldingBuf[2] = 99;
			}
			break;
		default:
			break;
		
	}
}

/***************************************************************************
*
* 步进电机3，4  地址40003
* 0：复位   1：下移0.1mm   2：下移1mm     3：上移0.1mm    4：上移1mm    
* 5：向上移动基准值
*
*****************************************************************************/

void motor_steptf()
{

	uint8_t id = 0;
		
		// 断点控制
	for(id = 0;  id < CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_SIZE; id++)
	{
		if(g_plc_flow_test_motor_port.node_buf[id].stat == TRUE)
			return;
	}
	
	switch(pusRegHoldingBuf[4])
	{
		//向下复位
		case 0:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[4] = 98;
			
				if(plc_input_read_flow(2) != 0)//E3保护
				{
					if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
					{
						plc_motor_set_step_rst_flow(2, 0,\
						8000, 500, 0.1,\
						2, 0, 0, -1,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
					}	
					
					if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
					{
						plc_motor_set_step_rst_flow(3, 0,\
						8000, 500, 0.1,\
						2, 0, 0, -1,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);	
						
					}
					flag_tf = 0;
				}
				
				//pusRegHoldingBuf[4] = 99;	
			}
			break;
		
		//下移1毫米
		case 2:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[4] = 98;
			
				if(plc_input_read_flow(2) != 0)//下移E3保护
				{
					flag_tf = flag_tf - 400;
					plc_motor_set_step_pos_flow(2, flag_tf,\
						8000, 500, 0.4,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
						
					plc_motor_set_step_pos_flow(3, flag_tf,\
						8000, 500, 0.4,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);		
				
				}
				
				//pusRegHoldingBuf[4] = 99;
			}
			break;
			
		//上移1毫米
		case 4:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[4] = 98;
			
				if(plc_input_read_flow(3) != 0)//上移E4限位保护
				{
					flag_tf = flag_tf + 400;
					
					plc_motor_set_step_pos_flow(2, flag_tf,\
					8000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
					
					plc_motor_set_step_pos_flow(3, flag_tf,\
					8000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
				}
				
				//pusRegHoldingBuf[4] = 99;
			}
			break;
			
		//复位后平调基准值
		case 5:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[4] = 98;
			
				if(plc_input_read_flow(3) != 0)
				{
					flag_tf = 26520;
					plc_motor_set_step_pos_flow(2, flag_tf,\
					8000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
					
					plc_motor_set_step_pos_flow(3, flag_tf,\
					8000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
				}
				
				//pusRegHoldingBuf[4] = 99;
			}
			break;
			
		case 6:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[4] = 98;
			
				if(plc_input_read_flow(3) != 0)
				{
					flag_tf = 26520;
					plc_motor_set_step_pos_flow(2, flag_tf,\
					8000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
					
					plc_motor_set_step_pos_flow(3, flag_tf,\
					8000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
				}
				
				//pusRegHoldingBuf[4] = 99;
			}
			break;
			
		case 20:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[4] = 98;
			
				if(plc_input_read_flow(3) != 0)
				{
					flag_tf = (pusRegHoldingBuf[18]*256 + pusRegHoldingBuf[32]);
					
					plc_motor_set_step_pos_flow(2, flag_tf,\
					12000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
					
					plc_motor_set_step_pos_flow(3, flag_tf,\
					12000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
				}
				
				//pusRegHoldingBuf[4] = 99;
			}
			break;
		default:
			break;
	}
}


/***************************************************************************
*
* 步进电机5  地址4000b
* 0：复位   
*
*****************************************************************************/
void motor_step_waste_M5()
{
	uint8_t id = 0;
	
	for(id = 0;  id < CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_SIZE; id++)
	{
		if(g_plc_flow_test_motor_port.node_buf[id].stat == TRUE)
			return;
	}
	
	switch(pusRegHoldingBuf[20])
	{
		case 0:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[20] = 98;		//开始对4000b操作
				//横杆M5复位
				if(plc_input_read_flow(10) != 0)	//没到限位开关，继续复位
				{
					//速度64000改为500，慢点安全
					plc_motor_set_step_rst_flow(4, 0,\
						5000, 500, 0.4,\
						10, 0, 0,1,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
					
				}
				flag_motor_five = 0;
				//pusRegHoldingBuf[20] = 99;		//对4000b操作结束
			}
			break;
		//通用上位机控制
		case 20:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[20] = 98;
			
				if(plc_input_read_flow(11) != 0)//E1限位
				{
					flag_motor_five = 0 - ((pusRegHoldingBuf[18]*256 + pusRegHoldingBuf[32])*10);
					 
					plc_motor_set_step_pos_flow(4, flag_motor_five,\
						49000, 100, 0.2,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
						
				}
				
				//pusRegHoldingBuf[20] = 99;
			}
			break;
		default:
			break;
	}
}

/***************************************************************************
*
* 步进电机6  地址4000c
* 0：复位   
*
*****************************************************************************/
void motor_step_waste_M6()
{
	uint8_t id = 0;
	
	for(id = 0;  id < CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_SIZE; id++)
	{
		if(g_plc_flow_test_motor_port.node_buf[id].stat == TRUE)
			return;
	}
	
	switch(pusRegHoldingBuf[22])
	{
		case 0:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[22] = 98;		//开始对4000c操作
				//横杆M5复位
				if(plc_input_read_flow(8) != 0)		//没到限位开关，继续复位
				{
					//速度8000改为500，慢点安全
					plc_motor_set_step_rst_flow(5, 0,\
						3000, 500, 0.4,\
						8, 0, 0,1,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
						flag_motor_six = 0;
				}
				//pusRegHoldingBuf[22] = 99;		//对4000c操作结束
			}
			break;
		//通用上位机控制
		case 20:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[22] = 98;
			
				if(plc_input_read_flow(9) != 0)//E1限位
				{
					flag_motor_six = 0 - ((pusRegHoldingBuf[18]*256 + pusRegHoldingBuf[32])*10);
					
					plc_motor_set_step_pos_flow(5, flag_motor_six,\
						45000, 500, 0.4,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
						
				}
				
				//pusRegHoldingBuf[22] = 99;
			}
			break;	
		default:
			break;
	}
}

/***************************************************************************
*
* 步进电机7  地址4000d
* 0：复位   
*
*****************************************************************************/
void motor_step_waste_M7()
{
	uint8_t id = 0;
	
	for(id = 0;  id < CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_SIZE; id++)
	{
		if(g_plc_flow_test_motor_port.node_buf[id].stat == TRUE)
			return;
	}
	
	switch(pusRegHoldingBuf[24])
	{
		case 0:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[24] = 98;		//开始对4000d操作
				//横杆M5复位
				if(plc_input_read_flow(12) != 0)	//没到限位开关，继续复位
				{
					//速度8000改为500，慢点安全
					plc_motor_set_step_rst_flow(6, 0,\
						6000, 500, 0.4,\
						12, 0, 0,1,\
						CONFIG_PLC_FEEDBACK_FLOW, 0,\
						g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
					
				}
				flag_motor_seven = 0;
				//pusRegHoldingBuf[24] = 99;		//对4000d操作结束
			}
			break;
		//通用上位机控制
		case 20:
			if(plc_motor_cur_status(g_plc_flow_test_motor_port.id) == 0)
			{
				pusRegHoldingBuf[24] = 98;
			
				if(plc_input_read_flow(13) != 0)//E1限位
				{
					flag_motor_seven = 0 - ((pusRegHoldingBuf[18]*256 + pusRegHoldingBuf[32])*10);
					
					plc_motor_set_step_pos_flow(6, flag_motor_seven,\
					25000, 500, 0.4,\
					CONFIG_PLC_FEEDBACK_FLOW, 0,\
					g_plc_flow_test_motor_port.node_buf, &g_plc_flow_test_motor_port.node_pos, CONFIG_PLC_TEST_MOTOR_FLOW_NODE_BUF_MASK);
						
				}
				
				//pusRegHoldingBuf[24] = 99;
			}
			break;	
		default:
			break;
	}
}

void button_con()
{
	if(plc_input_read(4) == 0)//开启按键
	{
		pusRegHoldingBuf[0] = 1;
	}
	
	if(plc_input_read(5) == 0)//停止按键
	{
		pusRegHoldingBuf[0] = 2;
	}
	
	if(plc_input_read(6) == 0)//停止按键
	{
		pusRegHoldingBuf[0] = 3;
	}
}

/*
                   _ooOoo_
                  o8888888o
                  88" . "88
                  (| -_- |)
                  O\  =  /O
               ____/`---'\____
             .'  \\|     |//  `.
            /  \\|||  :  |||//  \
           /  _||||| -:- |||||-  \
           |   | \\\  -  /// |   |
           | \_|  ''\---/''  |   |
           \  .-\__  `-`  ___/-. /
         ___`. .'  /--.--\  `. . __
      ."" '<  `.___\_<|>_/___.'  >'"".
     | | :  `- \`.;`\ _ /`;.`/ - ` : | |
     \  \ `-.   \_ __\ /__ _/   .-` /  /
======`-.____`-.___\_____/___.-`____.-'======
*/
