#ifndef __PLC_EEP_H__
#define __PLC_EEP_H__

#include "plc_type.h"
#include "plc_cfg.h"

// 初始化
void plc_eep_init(uint8_t id, uint8_t dev_addr, I2C_HandleTypeDef *hi2c);

// 读内容
uint8_t plc_eep_read_value(uint8_t id, uint16_t address, uint8_t size, uint32_t *value);
uint8_t plc_eep_read_buf(uint8_t id, uint16_t address, uint8_t size, uint8_t *buf);
	
// 写内容
uint8_t plc_eep_write_value(uint8_t id, uint16_t address, uint8_t size, uint32_t value);
uint8_t plc_eep_write_buf(uint8_t id, uint16_t address, uint8_t size, uint8_t *buf);
	
// modbus
#define CONFIG_PLC_EEP_MODBUS_LEN		16
int8_t plc_eep_modbus_write(uint8_t address, uint8_t *buf, uint8_t pos, uint8_t mask);
int8_t plc_eep_modbus_read(uint8_t address, uint8_t *buf, uint8_t pos, uint8_t mask);


#endif
