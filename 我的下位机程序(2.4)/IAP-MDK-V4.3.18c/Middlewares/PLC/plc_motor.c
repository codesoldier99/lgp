#include "plc_test.h"
#include "plc_motor.h"
#include "plc_input.h"
#include "app_tim.h"

#include <stdlib.h>

#include "main.h"

//static uint8_t motorFinishRunFlag = FALSE;

typedef enum{
	CONFIG_PLC_MOTOR_ISRUNNING_FREE	= 0,
	CONFIG_PLC_MOTOR_ISRUNNING_RUN,
	CONFIG_PLC_MOTOR_ISRUNNING_STOP,
	CONFIG_PLC_MOTOR_ISRUNNING_BTN_STOP,
	CONFIG_PLC_MOTOR_ISRUNNING_ACT_STOP,
	CONFIG_PLC_MOTOR_ISRUNNING_RELEASE,
}PLC_MOTOR_ISRUNNING;

typedef enum{
	CONFIG_PLC_MOTOR_CLEAN_POS_NO = 0,
	CONFIG_PLC_MOTOR_CLEAN_POS_YES,
}PLC_MOTOR_CLEAN_POS;


typedef enum{
	CONFIG_PLC_MOTOR_FREQ_STAGE_UP = 0,
	CONFIG_PLC_MOTOR_FREQ_STAGE_DOWN,
}PLC_MOTOR_FREQ_STAGE;

typedef enum{
	CONFIG_PLC_MOTOR_RUN_DIR_M = -1,
	CONFIG_PLC_MOTOR_RUN_DIR_P = 1,
}PLC_MOTOR_RUN_DIR;

#define	CONFIG_PLC_MOTOR_FREQ_SCALE 0.01


#define CFG_MOTOR_LOCK_TRIGGER_COUNT		5	//光电开关连续触发次数（光电开关用来做电机锁）

#define CONFIG_PLC_MOTOR_LEVEL_BUF_SIZE		8
#define CONFIG_PLC_MOTOR_LEVEL_BUF_MASK		(CONFIG_PLC_MOTOR_LEVEL_BUF_SIZE - 1)

#define CONFIG_MOTOR_LEV_BUF_POS(POS, OFFSET)	((POS + OFFSET) & CONFIG_PLC_MOTOR_LEVEL_BUF_MASK)

typedef struct{
	float    freq;
	uint16_t period;
	int32_t  count;
}PLC_STEPMOTOR_LEVEL;

typedef struct
{
	uint16_t freq;
	int8_t   dir;
	uint8_t  speed;
	
	uint16_t period;
	uint16_t period_h;
	uint16_t period_l;
}PLC_BLDC_PORT;


typedef enum
{
	CONFIG_PLC_MOTOR_PORT_MODE_STEPMOTOR = 0,
	CONFIG_PLC_MOTOR_PORT_MODE_BLDC,
}PLC_MOTOR_PORT_MODE;

typedef struct
{	
	uint8_t   isRunning;
	
	uint8_t   mode;	//步进电机 或 无刷直流电机
	
	int32_t   pos_dst;		//矢量坐标
	int32_t   pos_cur;		//当前坐标
	int8_t    run_dir;		//方向值
	
	uint16_t  freq_sta;		//启动速度
	uint16_t  freq_dst;		//目标速度
	float     freq_chg;		//变化率：百分数
	
	
	PLC_BLDC_PORT g_plc_bldc_port;	//无刷直流电机
	
	//定点
	int8_t    input_id;			//光电开关地址
	uint8_t   input_state;	//光电开关触发状态
	uint8_t   input_count;	//光电开关触发计数
	uint16_t  rst_pul_cfg;	//减速配置
	uint16_t  rst_pul_cnt;	//减速计数
	
	//定位或定点
	void    (*plc_motor_pul_action)(uint8_t id);
	uint8_t (*plc_motor_curve_buf)(uint8_t id);
	void    (*plc_motor_curve_init)(uint8_t id);
	
//流程端口
	uint8_t             feedback;
	PLC_TYPE_FLOW_INFO *p_flow_info;
	
	//通信端口
	uint8_t modbus_port_id;
	
	//阶段参数
	PLC_STEPMOTOR_LEVEL level_buf_val[CONFIG_PLC_MOTOR_LEVEL_BUF_SIZE];
	uint8_t             level_buf_pos;
	uint8_t             level_buf_cnt;
	int32_t             level_accel_count;
	int32_t             level_len;
	int32_t             cur_pul_cnt;
	
	PLC_STEPMOTOR_LEVEL *p_level_act;
	
	// 定时器
	TIM_HandleTypeDef   *htim;
	 int8_t              timer_index;
}PLC_MOTOR_PORT;
PLC_MOTOR_PORT g_plc_motor_port[CONFIG_PLC_MOTOR_PORT_COUNT];

/*
**************************************************************************************
* 定时器
**************************************************************************************
*/
#define  CONFIG_MOTOR_TIMER_COUNT	   10
int8_t             timer_motor_map[CONFIG_MOTOR_TIMER_COUNT]  = {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1};
TIM_HandleTypeDef *timer_motor_htim[CONFIG_MOTOR_TIMER_COUNT] = {&htim5, &htim6, &htim7, &htim8, &htim9, &htim10, &htim11, &htim12, &htim13, &htim14};

	uint8_t plc_motor_timer_insert(int8_t id)
{
	uint8_t timer_id;
	
	for(timer_id = 0; timer_id < CONFIG_MOTOR_TIMER_COUNT; timer_id++){
		if(timer_motor_map[timer_id] == -1){
			timer_motor_map[timer_id] = id;
//			g_plc_motor_port[stepmotor_id].htim = timer_motor_htim[timer_id];
			switch(timer_id){
				case 0:  g_plc_motor_port[id].htim = &htim5; break;
				case 1:  g_plc_motor_port[id].htim = &htim6; break;
				case 2:  g_plc_motor_port[id].htim = &htim7; break;
				case 3:  g_plc_motor_port[id].htim = &htim8; break;
				case 4:  g_plc_motor_port[id].htim = &htim9; break;
				case 5:  g_plc_motor_port[id].htim = &htim10; break;
				case 6:  g_plc_motor_port[id].htim = &htim11; break;
				case 7:  g_plc_motor_port[id].htim = &htim12; break;
				case 8:  g_plc_motor_port[id].htim = &htim13; break;
				case 9:  g_plc_motor_port[id].htim = &htim14; break;
			}
			
			g_plc_motor_port[id].timer_index = timer_id;
			HAL_TIM_Base_Start_IT(g_plc_motor_port[id].htim);
			return TRUE;
		}
	}
	
	return FALSE;
}

void plc_motor_timer_deletc(int8_t id)
{	
	if(g_plc_motor_port[id].timer_index != -1)
	{
		timer_motor_map[g_plc_motor_port[id].timer_index] = -1;
		g_plc_motor_port[id].timer_index                  = -1;
		HAL_TIM_Base_Stop_IT(g_plc_motor_port[id].htim);
	}
}


/*
******************************************************************
* 初始化
******************************************************************
*/
void plc_motor_init(void)
{
	uint8_t id = 0;
	
	for(id = 0; id < CONFIG_PLC_MOTOR_PORT_COUNT; id++){
		g_plc_motor_port[id].timer_index = -1;
	}
}	

/*
*****************************************************************************************************
* 反馈信号
*****************************************************************************************************
*/
void plc_motor_feedback(uint8_t id)
{
	PLC_MOTOR_PORT *p_motor_port = g_plc_motor_port + id;
	
	if(p_motor_port->feedback == CONFIG_PLC_FEEDBACK_UART){
		mb_rtu_out(2, 17, CONFIG_PLC_MB_ADDRESS_STEPMOTOR + id*CONFIG_PLC_MOTOR_MODBUS_LEN, CONFIG_PLC_MOTOR_MODBUS_LEN/2);
	}
	else if(p_motor_port->feedback == CONFIG_PLC_FEEDBACK_FLOW){
		PLC_TYPE_FLOW_INFO *p_flow_info = p_motor_port->p_flow_info;
		p_flow_info->stat = FALSE;
	}
}

/*
******************************************************************
* 设置方向
******************************************************************
*/
void plc_motor_set_dir(uint8_t id, int8_t run_dir)//dir的方向信号
{	
	if(run_dir == 1)
		HAL_GPIO_WritePin(GPIOF, 1<<id, GPIO_PIN_SET);
	else
		HAL_GPIO_WritePin(GPIOF, 1<<id, GPIO_PIN_RESET);
}

void plc_motor_set_pul(uint8_t id, GPIO_PinState state)
{	
	HAL_GPIO_WritePin(GPIOE, 1<<id, state);
}

/*
******************************************************************
* 变量初始化
******************************************************************
*/
void plc_motor_curve_init_pos(uint8_t id)
{
	PLC_MOTOR_PORT *p_motor_port = g_plc_motor_port + id;
	
	p_motor_port->level_buf_cnt     = 0;
	p_motor_port->level_buf_pos     = 1;
	p_motor_port->level_accel_count = 0;
	
	if(p_motor_port->run_dir == 1){
		p_motor_port->level_len = p_motor_port->pos_dst - p_motor_port->pos_cur;	
	}else{
		p_motor_port->level_len = p_motor_port->pos_cur - p_motor_port->pos_dst;
	}
	
	p_motor_port->level_buf_val[0].freq   = p_motor_port->freq_sta;
	p_motor_port->level_buf_val[0].period = 1000000/p_motor_port->level_buf_val[0].freq/2;
	p_motor_port->level_buf_val[0].count  = p_motor_port->level_buf_val[0].freq*CONFIG_PLC_MOTOR_FREQ_SCALE;
	p_motor_port->level_len -= p_motor_port->level_buf_val[0].count;
	
	p_motor_port->p_level_act        = p_motor_port->level_buf_val + 0;	
	p_motor_port->cur_pul_cnt        = p_motor_port->p_level_act->count;
	
	plc_motor_timer_insert(id);
}

void plc_motor_curve_init_rst(uint8_t id)
{
	PLC_MOTOR_PORT *p_motor_port = g_plc_motor_port + id;
	
	p_motor_port->level_buf_cnt     = 0;
	p_motor_port->level_buf_pos     = 1;
	p_motor_port->level_accel_count = 0;
	
	p_motor_port->level_buf_val[0].freq   = p_motor_port->freq_sta;
	p_motor_port->level_buf_val[0].period = 500000/p_motor_port->level_buf_val[0].freq;
	p_motor_port->level_buf_val[0].count  = p_motor_port->level_buf_val[0].freq*CONFIG_PLC_MOTOR_FREQ_SCALE;
	
	p_motor_port->p_level_act = &p_motor_port->level_buf_val[0];
	p_motor_port->cur_pul_cnt = p_motor_port->p_level_act->count;
	
	plc_motor_timer_insert(id);
}

/*
******************************************************************
* 加减速曲线
******************************************************************
*/
uint8_t plc_motor_curve_buf_pos(uint8_t id)
{
	PLC_MOTOR_PORT *p_motor_port = g_plc_motor_port + id;
	
	if(p_motor_port->level_buf_pos == p_motor_port->level_buf_cnt)
		return FALSE;
		
	PLC_STEPMOTOR_LEVEL *p_level_buf_cur = &(p_motor_port->level_buf_val[p_motor_port->level_buf_pos]);
	PLC_STEPMOTOR_LEVEL *p_level_buf_lst = &(p_motor_port->level_buf_val[CONFIG_MOTOR_LEV_BUF_POS(p_motor_port->level_buf_pos, -1)]);
	
	//加速或匀速阶段
	if(p_motor_port->level_accel_count >= p_motor_port->level_len ||\
		p_motor_port->isRunning == CONFIG_PLC_MOTOR_ISRUNNING_BTN_STOP){ //减速
		p_level_buf_cur->freq = p_level_buf_lst->freq/(1 + p_motor_port->freq_chg);
		
		if(p_level_buf_cur->freq > p_motor_port->freq_sta){
			p_level_buf_cur->count  = p_level_buf_cur->freq*CONFIG_PLC_MOTOR_FREQ_SCALE;
		}else{
			p_level_buf_cur->freq  = p_motor_port->freq_sta;
			if(p_motor_port->isRunning == CONFIG_PLC_MOTOR_ISRUNNING_BTN_STOP)
				p_level_buf_cur->count = p_level_buf_cur->freq*CONFIG_PLC_MOTOR_FREQ_SCALE;
			else
				p_level_buf_cur->count = p_motor_port->level_len;
		}
	}else{
		p_level_buf_cur->freq = p_level_buf_lst->freq*(1 + p_motor_port->freq_chg);
		
		if(p_level_buf_cur->freq < p_motor_port->freq_dst){	//加速
			p_level_buf_cur->count = p_level_buf_cur->freq*CONFIG_PLC_MOTOR_FREQ_SCALE;	
			p_motor_port->level_accel_count += p_level_buf_cur->count;
		}else{	//匀速
			p_level_buf_cur->freq = p_motor_port->freq_dst;
			p_level_buf_cur->count = p_motor_port->level_len - p_motor_port->level_accel_count;
		}
	}
	p_level_buf_cur->period         = 500000/p_level_buf_cur->freq;
	p_motor_port->level_len    -= p_level_buf_cur->count;
	p_motor_port->level_buf_pos = CONFIG_MOTOR_LEV_BUF_POS(p_motor_port->level_buf_pos, 1);
	
	return TRUE;
}

uint8_t plc_motor_curve_buf_rst(uint8_t id)
{
	PLC_MOTOR_PORT *p_motor_port = g_plc_motor_port + id;
	
	if(p_motor_port->level_buf_pos == p_motor_port->level_buf_cnt)
		return FALSE;
	
	PLC_STEPMOTOR_LEVEL *p_level_buf_cur = &(p_motor_port->level_buf_val[p_motor_port->level_buf_pos]);
	PLC_STEPMOTOR_LEVEL *p_level_buf_lst = &(p_motor_port->level_buf_val[CONFIG_MOTOR_LEV_BUF_POS(p_motor_port->level_buf_pos, -1)]);

	if(p_motor_port->isRunning == CONFIG_PLC_MOTOR_ISRUNNING_ACT_STOP ||\
		p_motor_port->isRunning == CONFIG_PLC_MOTOR_ISRUNNING_BTN_STOP){	//减速
		p_level_buf_cur->freq = p_level_buf_lst->freq/(1 + p_motor_port->freq_chg);
		
		if(p_level_buf_cur->freq > p_motor_port->freq_sta){
			p_level_buf_cur->count  = p_level_buf_cur->freq*CONFIG_PLC_MOTOR_FREQ_SCALE;
		}else{
			p_level_buf_cur->freq = p_motor_port->freq_sta;
			p_level_buf_cur->count = p_motor_port->level_len;
		}
	}else{
		p_level_buf_cur->freq = p_level_buf_lst->freq*(1 + p_motor_port->freq_chg);
		if(p_level_buf_cur->freq >= p_motor_port->freq_dst){	//匀速
			p_level_buf_cur->freq   = p_motor_port->freq_dst;
			p_level_buf_cur->count  = 0x7FFFFFFF;
		}else{	//加速
			p_level_buf_cur->count = p_level_buf_cur->freq*CONFIG_PLC_MOTOR_FREQ_SCALE;
		}			
	}
	
	p_level_buf_cur->period = 500000/p_level_buf_cur->freq;
	p_motor_port->level_buf_pos = CONFIG_MOTOR_LEV_BUF_POS(p_motor_port->level_buf_pos, 1);
	
	return TRUE;
}


/*
*****************************************************************************************************
* 步进电机运行 - 运行
*****************************************************************************************************
*/

void plc_motor_scan(void)
{
	uint8_t             id  = 0;
	PLC_MOTOR_PORT *p_motor_port;
	
	for(id = 0; id < CONFIG_PLC_MOTOR_PORT_COUNT; id++){
		p_motor_port = g_plc_motor_port + id;
		
		if(g_plc_motor_port[id].mode != CONFIG_PLC_MOTOR_PORT_MODE_STEPMOTOR)
			continue;
		
		if(p_motor_port->isRunning == CONFIG_PLC_MOTOR_ISRUNNING_RELEASE){
			plc_motor_feedback(id);
			p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_FREE;
		}else if(p_motor_port->isRunning != CONFIG_PLC_MOTOR_ISRUNNING_FREE){
			p_motor_port->plc_motor_curve_buf(id);
		}
	}
}

/*
***********************************************************************************************
* 定时器
***********************************************************************************************
*/
void plc_motor_freq_down(PLC_MOTOR_PORT *p_motor_port)
{
	PLC_STEPMOTOR_LEVEL *p_level_buf;
	
	p_motor_port->level_buf_cnt = CONFIG_MOTOR_LEV_BUF_POS(p_motor_port->level_buf_cnt, 1);
	p_motor_port->level_buf_pos = CONFIG_MOTOR_LEV_BUF_POS(p_motor_port->level_buf_cnt, 1);		
	
	p_level_buf = &(p_motor_port->level_buf_val[p_motor_port->level_buf_pos]);
	
	p_level_buf->freq = p_motor_port->p_level_act->freq/(1 + p_motor_port->freq_chg);	
	if(p_level_buf->freq < p_motor_port->freq_sta){
		p_level_buf->freq = p_motor_port->freq_sta;
	}p_level_buf->count = p_level_buf->freq*CONFIG_PLC_MOTOR_FREQ_SCALE;
		
	p_level_buf->period = 500000/p_level_buf->freq;
	p_motor_port->p_level_act = p_level_buf;
}


void plc_motor_pul_action_pos(uint8_t id)
{
	PLC_MOTOR_PORT *p_motor_port = g_plc_motor_port + id;
	
	if(--p_motor_port->cur_pul_cnt <= 0){
		if(p_motor_port->isRunning == CONFIG_PLC_MOTOR_ISRUNNING_BTN_STOP){
			if(p_motor_port->p_level_act->freq <= p_motor_port->freq_sta){
				plc_motor_timer_deletc(id);
				p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_STOP;
				return;
			}
		}
		
		p_motor_port->level_buf_cnt = CONFIG_MOTOR_LEV_BUF_POS(p_motor_port->level_buf_cnt, 1);		
		p_motor_port->p_level_act   = p_motor_port->level_buf_val + p_motor_port->level_buf_cnt;		
		p_motor_port->cur_pul_cnt   = p_motor_port->p_level_act->count;
	}
	p_motor_port->pos_cur += p_motor_port->run_dir;
	
	/*******控制拍照时机*************************************************************************/
	//M1在拍照范围内
	if((id ==0) && (p_motor_port->pos_cur <= 20605) && (p_motor_port->pos_cur >= 5545))
	{
		flag_camera_M1 =1;
	}
	else
	{
		flag_camera_M1 =0;
	}
	
	if(flag_camera_M1 == 1)
	{
		
			if(p_motor_port->pos_cur == 5545)
			{
				pos = p_motor_port->pos_cur;	//给pos赋初值
				Close_read_camera();			//停止上位机读取照片
			}
			else if(p_motor_port->pos_cur == 20605)
			{
				pos = p_motor_port->pos_cur;	//给pos赋初值
				Close_read_camera();			//停止上位机读取照片
			}
		
		//判断电机M1前进还是后退
		if(id ==0 && p_motor_port->run_dir > 0)
		{
			
			//M1前进
			if(p_motor_port->pos_cur - pos==364)
			{
				flag_camera = 1;
				pos = p_motor_port->pos_cur;
				flag_camera = plc_motor_pos_flag(flag_camera);
			}
		}
		else if(id ==0 && p_motor_port->run_dir < 0)
		{
			//M1后退
			if( pos - p_motor_port->pos_cur ==364 )
			{
				flag_camera = 1;
				pos = p_motor_port->pos_cur;
				flag_camera = plc_motor_pos_flag(flag_camera);
			}
		}
	}
	
	
/**********************************************************************************/
	
	if(p_motor_port->pos_cur == p_motor_port->pos_dst){
		
		finishRun(id);
		plc_motor_timer_deletc(id);
		p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_RELEASE;
	}
	
}

void plc_motor_pul_action_rst(uint8_t id)
{
	PLC_MOTOR_PORT *p_motor_port = g_plc_motor_port + id;//PLC_MOTOR_PORT的一个结构体变量
	
	if(p_motor_port->input_count < CFG_MOTOR_LOCK_TRIGGER_COUNT){//光电开关的计数器小于连续触发次数
		if(plc_input_read(p_motor_port->input_id) == p_motor_port->input_state)//读出状态（光电开关的地址）=光电开关触发状态
			p_motor_port->input_count++;
		else
			p_motor_port->input_count = 0;
	}else if( p_motor_port->input_count == CFG_MOTOR_LOCK_TRIGGER_COUNT ){
		plc_motor_freq_down(p_motor_port);
		//finishRst(id);	//电机复位完毕，相应寄存器值变为99
		p_motor_port->rst_pul_cnt = 0;
		p_motor_port->input_count++;
		
		p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_ACT_STOP;
	}else{
		if(++p_motor_port->rst_pul_cnt >= p_motor_port->rst_pul_cfg){
			if(p_motor_port->pos_dst >= 0){
				p_motor_port->pos_cur = p_motor_port->pos_dst;
			}
			
			plc_motor_timer_deletc(id);
			p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_RELEASE;
			return;
		}
	}
	
	if(--p_motor_port->cur_pul_cnt <= 0){
		if(p_motor_port->isRunning == CONFIG_PLC_MOTOR_ISRUNNING_BTN_STOP &&\
			p_motor_port->p_level_act->freq <= p_motor_port->freq_sta){
			plc_motor_timer_deletc(id);
			p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_STOP;
			return;
		}

		p_motor_port->level_buf_cnt = CONFIG_MOTOR_LEV_BUF_POS(p_motor_port->level_buf_cnt, 1);	
		p_motor_port->p_level_act   = p_motor_port->level_buf_val + p_motor_port->level_buf_cnt;
		p_motor_port->cur_pul_cnt   = p_motor_port->p_level_act->count;
	}
	
	p_motor_port->pos_cur += p_motor_port->run_dir;
}

inline void plc_motor_pul_action(uint8_t timer)	// 步进电机
{
	int8_t id = timer_motor_map[timer];
	
	// 电平翻转
	GPIOE->ODR ^= 1<<id;
	
	if(g_plc_motor_port[id].mode == CONFIG_PLC_MOTOR_PORT_MODE_STEPMOTOR){	
		// 下一个脉冲
		if((GPIOE->IDR & (1<<id)) != (uint32_t)GPIO_PIN_RESET){
			g_plc_motor_port[id].plc_motor_pul_action(id);//定位定点
		}
		
		// 定时器
		g_plc_motor_port[id].htim->Instance->ARR = g_plc_motor_port[id].p_level_act->period + 1;
	}else if(g_plc_motor_port[id].mode == CONFIG_PLC_MOTOR_PORT_MODE_BLDC){
		// 计时翻转
		if((GPIOE->IDR & (1<<id)) != (uint32_t)GPIO_PIN_RESET){
			g_plc_motor_port[id].htim->Instance->ARR = g_plc_motor_port[id].g_plc_bldc_port.period_h;
		}else{
			g_plc_motor_port[id].htim->Instance->ARR = g_plc_motor_port[id].g_plc_bldc_port.period_l;
		}
	}
}

/*
*****************************************************************************************************
* 状态
*****************************************************************************************************
*/
uint8_t plc_motor_cur_status(uint8_t id)//当前状态
{
	return g_plc_motor_port[id].isRunning;
}

int32_t plc_motor_cur_postion(uint8_t id)
{
	return g_plc_motor_port[id].pos_cur;
}

/*
*****************************************************************************************************
* 按键响应
*****************************************************************************************************
*/
void plc_button_run_motor(void)
{
	uint8_t id;
	PLC_MOTOR_PORT *p_motor_port;
	
	for(id = 0; id < CONFIG_PLC_MOTOR_PORT_COUNT; id++){
		p_motor_port = g_plc_motor_port + id;	
		
		if(p_motor_port->isRunning == CONFIG_PLC_MOTOR_ISRUNNING_STOP){
			if(p_motor_port->mode == CONFIG_PLC_MOTOR_PORT_MODE_STEPMOTOR){
				p_motor_port->plc_motor_curve_init(id);
			}else if(p_motor_port->mode == CONFIG_PLC_MOTOR_PORT_MODE_BLDC){
				HAL_TIM_Base_Start_IT(p_motor_port->htim);
			}
			
			p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_RUN;
		}
	}
}

void plc_button_stop_motor(void)
{
	uint8_t id;
	PLC_MOTOR_PORT *p_motor_port;
	
	for(id = 0; id < CONFIG_PLC_MOTOR_PORT_COUNT; id++){
		p_motor_port = g_plc_motor_port + id;
		
		if(p_motor_port->isRunning == CONFIG_PLC_MOTOR_ISRUNNING_RUN){			
			if(p_motor_port->mode == CONFIG_PLC_MOTOR_PORT_MODE_STEPMOTOR){
				p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_BTN_STOP;	// 暂停标志位
				
				p_motor_port->level_buf_pos = CONFIG_MOTOR_LEV_BUF_POS(p_motor_port->level_buf_cnt, 1);						
				p_motor_port->plc_motor_curve_buf(id);	// 曲线重构
				p_motor_port->level_buf_cnt = CONFIG_MOTOR_LEV_BUF_POS(p_motor_port->level_buf_cnt, 1);
				p_motor_port->p_level_act   = p_motor_port->level_buf_val + p_motor_port->level_buf_cnt;
				p_motor_port->cur_pul_cnt   = p_motor_port->p_level_act->count;
			}else if(p_motor_port->mode == CONFIG_PLC_MOTOR_PORT_MODE_BLDC){
				p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_STOP;	// 暂停标志位
				
				HAL_TIM_Base_Stop_IT(p_motor_port->htim);
				plc_motor_set_pul(id, GPIO_PIN_RESET);
			}
		}
	}
}

/*
*****************************************************************************************************
* 逻辑控制
*****************************************************************************************************
*/
void plc_motor_set_step_pos_flow(uint8_t id, int32_t pos_dst,\
	uint16_t freq_dst, uint16_t freq_sta, float freq_chg,\
	uint8_t feedback, uint8_t modbus_port_id,\
	PLC_TYPE_FLOW_INFO *p_flow_info, uint8_t *p_flow_pos, uint8_t flow_mask)
{
	PLC_MOTOR_PORT *p_motor_port = g_plc_motor_port + id;
			
	if(pos_dst == p_motor_port->pos_cur)
		return;
	
	p_motor_port->pos_dst  = pos_dst;
		
	p_motor_port->freq_dst = freq_dst;
	p_motor_port->freq_sta = freq_sta;
	p_motor_port->freq_chg = freq_chg;
	
	p_motor_port->feedback   	   = feedback;
	p_motor_port->modbus_port_id = modbus_port_id;
	
	p_motor_port->cur_pul_cnt   = 0;
	
	//初始化运动方向
	if(p_motor_port->pos_dst > p_motor_port->pos_cur){// 目的坐标位置
		p_motor_port->run_dir = 1;
	}else{
		p_motor_port->run_dir = -1;
	}
	plc_motor_set_dir(id, p_motor_port->run_dir);
	
	//反馈
	if(feedback == CONFIG_PLC_FEEDBACK_FLOW){
		p_motor_port->p_flow_info = p_flow_info + *p_flow_pos;
		p_motor_port->p_flow_info->id   = id;
		p_motor_port->p_flow_info->port = 'M';
		p_motor_port->p_flow_info->stat = TRUE;
		
		*p_flow_pos = (*p_flow_pos + 1) & flow_mask;
	}
	
	p_motor_port->plc_motor_curve_buf  = plc_motor_curve_buf_pos;
	p_motor_port->plc_motor_pul_action = plc_motor_pul_action_pos;
	p_motor_port->plc_motor_curve_init = plc_motor_curve_init_pos;	
	
	p_motor_port->plc_motor_curve_init(id);
	
	p_motor_port->mode      = CONFIG_PLC_MOTOR_PORT_MODE_STEPMOTOR;
	p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_RUN;
}

void plc_motor_set_step_rst_flow(uint8_t id, int32_t pos_dst,\
	uint16_t freq_dst, uint16_t freq_sta, float freq_chg,\
	int8_t input_id, uint8_t input_state, uint16_t rst_pul_cfg, int8_t run_dir,\
	uint8_t feedback, uint8_t modbus_port_id,\
	PLC_TYPE_FLOW_INFO *p_flow_info, uint8_t *p_flow_pos, uint8_t flow_mask)
{
	PLC_MOTOR_PORT *p_motor_port = g_plc_motor_port + id;
	
	//相同位置
	if(plc_input_read(input_id) == input_state)
		return;
	
	p_motor_port->pos_dst    = pos_dst;
	
	p_motor_port->freq_dst   = freq_dst;
	p_motor_port->freq_chg   = freq_chg;
	p_motor_port->freq_sta   = freq_sta;
	
	p_motor_port->input_id    = input_id;
	p_motor_port->input_state = input_state;
	p_motor_port->input_count = 0;
	
	p_motor_port->run_dir     = run_dir;
	p_motor_port->rst_pul_cfg = rst_pul_cfg;
	p_motor_port->rst_pul_cnt = 0;
	
	p_motor_port->feedback   	= feedback;	
	p_motor_port->modbus_port_id = modbus_port_id;
	
	p_motor_port->cur_pul_cnt   = 0;
			
	//初始化运动方向
	plc_motor_set_dir(id, p_motor_port->run_dir);
	
	if(feedback == CONFIG_PLC_FEEDBACK_FLOW){
		p_motor_port->p_flow_info = p_flow_info + *p_flow_pos;
		p_motor_port->p_flow_info->id   = id;
		p_motor_port->p_flow_info->port = 'M';
		p_motor_port->p_flow_info->stat = TRUE;
		
		*p_flow_pos = (*p_flow_pos + 1) & flow_mask;
	}
	
	//跳转函数
	p_motor_port->plc_motor_curve_buf  = plc_motor_curve_buf_rst;
	p_motor_port->plc_motor_pul_action = plc_motor_pul_action_rst;
	p_motor_port->plc_motor_curve_init = plc_motor_curve_init_rst;	
	
	p_motor_port->plc_motor_curve_init(id);
	
	p_motor_port->mode      = CONFIG_PLC_MOTOR_PORT_MODE_STEPMOTOR;
	p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_RUN;
	

}

/*
**************************************************************************************
* 设置无刷直流电机
**************************************************************************************
*/
void plc_motor_set_bldc_flow(uint8_t id, uint32_t freq, int8_t dir, uint8_t speed)
{
	PLC_MOTOR_PORT *p_motor_port = g_plc_motor_port + id;
	PLC_BLDC_PORT      *p_plc_bldc_port = &p_motor_port->g_plc_bldc_port;
	
	if(p_plc_bldc_port->speed == speed && p_plc_bldc_port->dir == dir)
		return;
	
	plc_motor_timer_deletc(id);
	
	// 设置周期
	p_plc_bldc_port->freq     = freq;
	p_plc_bldc_port->period   = 1000000/freq;
	
	// 设置占空比
	p_plc_bldc_port->speed    = speed;
	p_plc_bldc_port->period_h = p_plc_bldc_port->period*speed/100;
	p_plc_bldc_port->period_l = p_plc_bldc_port->period - p_plc_bldc_port->period_h;
	
	// 设置方向
	p_plc_bldc_port->dir      = dir;
	plc_motor_set_dir(id, dir);
	
	// 设置模式
	p_motor_port->mode = CONFIG_PLC_MOTOR_PORT_MODE_BLDC;
	
	if(speed != 0)
	{
		p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_RUN;	// 标志位
		plc_motor_timer_insert(id);	// 插入定时器
	}else{		
		plc_motor_set_pul(id, GPIO_PIN_RESET);
		p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_FREE;	// 标志位
	}
}

/*
*****************************************************************************************************
* modbus
*****************************************************************************************************
*/

int8_t plc_motor_modbus_write(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask)
{
	if(id >= CONFIG_PLC_MOTOR_PORT_COUNT)
		return -1;
	
	int32_t cur_pos;
	
	PLC_MOTOR_PORT *p_motor_port = g_plc_motor_port + id;
	
	p_motor_port->mode       = buf[(pos + 0) & mask];
	
	if(p_motor_port->mode == CONFIG_PLC_MOTOR_PORT_MODE_STEPMOTOR){
		p_motor_port->feedback   = buf[(pos + 1) & mask];
		
		p_motor_port->isRunning  = buf[(pos + 2) & mask];
		
		p_motor_port->pos_dst = (buf[(pos + 4) & mask]<<24) + (buf[(pos + 5) & mask]<<16) + (buf[(pos + 6) & mask]<<8) + buf[(pos + 7) & mask];

		cur_pos = (buf[(pos + 8) & mask]<<24) + (buf[(pos + 9) & mask]<<16) + (buf[(pos + 10) & mask]<<8) + buf[(pos + 11) & mask];
		if(cur_pos >= 0){
			p_motor_port->pos_cur = cur_pos;
		}
		
		p_motor_port->freq_sta = (buf[(pos + 16) & mask]<<8) + buf[(pos + 17) & mask];
		p_motor_port->freq_dst = (buf[(pos + 18) & mask]<<8) + buf[(pos + 19) & mask];
		p_motor_port->freq_chg = ((float)buf[(pos + 20) & mask])/100;
		p_motor_port->run_dir  = buf[(pos + 21) & mask];
		
		p_motor_port->input_id    = buf[(pos + 22) & mask];
		p_motor_port->input_state = buf[(pos + 23) & mask];
		p_motor_port->rst_pul_cfg = (buf[(pos + 24) & mask]<<8) + buf[(pos + 25) & mask];
		
		if(p_motor_port->input_id >= 0){
			p_motor_port->rst_pul_cnt = 0;
			p_motor_port->input_count = 0;
			
			plc_motor_set_dir(id, p_motor_port->run_dir);				//初始化运动方向
			
			p_motor_port->plc_motor_pul_action = plc_motor_pul_action_rst;
			p_motor_port->plc_motor_curve_buf  = plc_motor_curve_buf_rst;
			p_motor_port->plc_motor_curve_init = plc_motor_curve_init_rst;
		}else{
			if(p_motor_port->pos_dst == p_motor_port->pos_cur){	//初始化运动方向
				return CONFIG_PLC_MOTOR_MODBUS_LEN;
			}
			if(p_motor_port->pos_dst > p_motor_port->pos_cur){	
				p_motor_port->run_dir   = 1;
			}else{
				p_motor_port->run_dir   = -1;
			}plc_motor_set_dir(id, p_motor_port->run_dir);		
			
			p_motor_port->plc_motor_curve_buf  = plc_motor_curve_buf_pos;
			p_motor_port->plc_motor_pul_action = plc_motor_pul_action_pos;
			p_motor_port->plc_motor_curve_init = plc_motor_curve_init_pos;	
		}
		
		p_motor_port->plc_motor_curve_init(id);	//初始化
		p_motor_port->isRunning = CONFIG_PLC_MOTOR_ISRUNNING_RUN;
	}else{	
		p_motor_port->freq_dst = (buf[(pos + 18) & mask]<<8) + buf[(pos + 19) & mask];	// 频率
		p_motor_port->freq_chg = ((float)buf[(pos + 20) & mask]);										// 速度，占空比
		p_motor_port->run_dir  = buf[(pos + 21) & mask];																// 方向
		
		plc_motor_set_bldc_flow(id, p_motor_port->freq_dst, p_motor_port->run_dir, p_motor_port->freq_chg);
	}
	return CONFIG_PLC_MOTOR_MODBUS_LEN;
}

int8_t plc_motor_modbus_read(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask)
{
	if(id >= CONFIG_PLC_MOTOR_PORT_COUNT)
		return -1;
	
	PLC_MOTOR_PORT *p_motor_port = g_plc_motor_port + id;
	
	buf[(pos + 1) & mask] = p_motor_port->feedback;
	
	buf[(pos + 2) & mask] = p_motor_port->isRunning;
	
	buf[(pos + 4) & mask] = (p_motor_port->pos_dst>>24) & 0xFF;
	buf[(pos + 5) & mask] = (p_motor_port->pos_dst>>16) & 0xFF;
	buf[(pos + 6) & mask] = (p_motor_port->pos_dst>>8)  & 0xFF;
	buf[(pos + 7) & mask] =  p_motor_port->pos_dst & 0xFF;
	
	buf[(pos + 8) & mask] = (p_motor_port->pos_cur>>24) & 0xFF;
	buf[(pos + 9) & mask] = (p_motor_port->pos_cur>>16) & 0xFF;
	buf[(pos + 10) & mask] = (p_motor_port->pos_cur>>8) & 0xFF;
	buf[(pos + 11) & mask] = p_motor_port->pos_cur & 0xFF;
		
	buf[(pos + 16) & mask] = (p_motor_port->freq_sta>>8) & 0xFF;
	buf[(pos + 17) & mask] =  p_motor_port->freq_sta & 0xFF;
	buf[(pos + 18) & mask] = (p_motor_port->freq_dst>>8) & 0xFF;
	buf[(pos + 19) & mask] = p_motor_port->freq_dst & 0xFF;
	
	buf[(pos + 20) & mask] = (uint8_t)(p_motor_port->freq_chg*100);
	buf[(pos + 21) & mask] = p_motor_port->run_dir;
	buf[(pos + 22) & mask] = p_motor_port->input_id;
	buf[(pos + 23) & mask] = p_motor_port->input_state;
	buf[(pos + 24) & mask] = (p_motor_port->rst_pul_cfg>>8) & 0xFF;
	buf[(pos + 25) & mask] = p_motor_port->rst_pul_cfg & 0xFF;
	
	return CONFIG_PLC_MOTOR_MODBUS_LEN;
}

