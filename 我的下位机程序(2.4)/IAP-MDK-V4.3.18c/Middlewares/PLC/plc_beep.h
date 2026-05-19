#ifndef __PLC_BEEP_H__
#define __PLC_BEEP_H__

#include "plc_type.h"

// É¨Ãè
void plc_beep_scan(void);

// modbus
#define CONFIG_PLC_BEEP_MODBUS_LEN	18
int8_t plc_beep_modbus_write(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask);
int8_t plc_beep_modbus_read(uint8_t id, uint8_t *buf, uint8_t pos, uint8_t mask);

// Âß¼­
void plc_beep_flow(uint8_t id,\
	uint8_t obj_state_cfg, uint32_t time_dly, uint32_t time_hold, uint32_t time_grid, uint8_t act_cnt_cfg,\
	uint8_t feedback, uint8_t modbus_port_id,\
	PLC_TYPE_FLOW_INFO *p_flow_info, uint8_t *p_flow_pos, uint8_t flow_mask);
	
#endif
