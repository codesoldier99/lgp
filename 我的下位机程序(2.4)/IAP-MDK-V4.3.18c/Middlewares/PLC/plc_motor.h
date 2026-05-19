#ifndef __PLC_STEPMOTOR_H__
#define __PLC_STEPMOTOR_H__

#include "plc_type.h"

static int flag_camera = 0;
static int flag_camera_M1 = 0;
static int flag_camera_M2 = 0;
static int flag_camera_M34 = 0;
static int pos = 0;

// 删除电机
void plc_motor_timer_deletc(int8_t id);

// 初始化
void plc_motor_init(void);
	
// 扫描
void plc_motor_scan(void);

// 定时器
void plc_motor_pul_action(uint8_t timer);

// 状态
uint8_t plc_motor_cur_status(uint8_t id);
int32_t plc_motor_cur_postion(uint8_t id);

// 逻辑控制
void plc_motor_set_step_pos_flow(uint8_t id, int32_t pos_dst,\
	uint16_t freq_dst, uint16_t freq_sta, float freq_chg,\
	uint8_t feedback, uint8_t modbus_port_id,\
	PLC_TYPE_FLOW_INFO *p_flow_info, uint8_t *p_flow_pos, uint8_t flow_mask);

void plc_motor_set_step_rst_flow(uint8_t id, int32_t pos_dst,\
	uint16_t freq_dst, uint16_t freq_sta, float freq_chg,\
	int8_t input_id, uint8_t input_state, uint16_t rst_pul_cfg, int8_t run_dir,\
	uint8_t feedback, uint8_t modbus_port_id,\
	PLC_TYPE_FLOW_INFO *p_flow_info, uint8_t *p_flow_pos, uint8_t flow_mask);

void plc_motor_set_bldc_flow(uint8_t id, uint32_t freq, int8_t dir, uint8_t speed);
	
// modbus
#define CONFIG_PLC_MOTOR_MODBUS_LEN	26

int8_t plc_motor_modbus_write(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask);
int8_t plc_motor_modbus_read(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask);
#endif
