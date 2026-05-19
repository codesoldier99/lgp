#ifndef __PLC_INPUT_H__
#define __PLC_INPUT_H__

#include "plc_type.h"
#include "plc_cfg.h"

// 初始化
void plc_input_init(void);

// 写入状态
void plc_input_write(uint8_t id, uint8_t state);

// 读出状态
uint8_t plc_input_read(int8_t id);
uint8_t plc_input_read_flow(int8_t id);

// 读出数值
uint16_t *plc_input_count(int8_t id);
uint16_t plc_input_count_two(int8_t id);

// 初始化
void plc_input_init(void);
	
// 扫描
void plc_input_scan(void);

// modbus

#define CONFIG_PLC_INPUT_MODBUS_LEN	10

int8_t plc_input_modbus_write(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask);
int8_t plc_input_modbus_read(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask);

#endif
