#ifndef __PLC_SWITCH_H__
#define __PLC_SWITCH_H__

#include "plc_type.h"
#include "plc_input.h"

// 端口初始化
void plc_switch_init(void);
	
// 设置电磁阀开关状态
void plc_switch_set_port(uint8_t id, uint8_t state);

// 扫描
void plc_switch_scan(void);
	
// modbus
#define CONFIG_PLC_SWITCH_MODBUS_LEN	20

int8_t plc_switch_modbus_write(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask);
int8_t plc_switch_modbus_read(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask);

// 逻辑控制
void plc_switch_set_flow(uint8_t id,\
	uint8_t obj_state_cfg, uint32_t time_dly, uint32_t time_hold, uint32_t time_grid, uint8_t act_cnt_cfg,\
	int8_t  input_id, uint8_t input_state,\
	uint8_t feedback, uint8_t modbus_port_id,\
	PLC_TYPE_FLOW_INFO *p_flow_info, uint8_t *p_flow_pos, uint8_t flow_mask);

#endif

