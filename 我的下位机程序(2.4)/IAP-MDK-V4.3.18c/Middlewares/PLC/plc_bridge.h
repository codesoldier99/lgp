#ifndef __PLC_DCMOTOR_H__
#define __PLC_DCMOTOR_H__

#include "plc_type.h"

// ¶Ë¿Ú³õÊ¼»¯
void plc_bridge_init(void);
	
// É¨Ãè
void plc_bridge_scan(void);
	
// modbus
#define CONFIG_PLC_BRIDGE_MODBUS_LEN	14

int8_t plc_bridge_modbus_write(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask);
int8_t plc_bridge_modbus_read(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask);

// Âß¼­¿ØÖÆ
void plc_bridge_flow(uint8_t id, uint16_t cfg_val, uint8_t dir, uint32_t time_dly, uint32_t time_hold,\
	uint8_t feedback, uint8_t modbus_port_id,\
	PLC_TYPE_FLOW_INFO *p_flow_info, uint8_t *p_flow_pos, uint8_t flow_mask);

#endif
