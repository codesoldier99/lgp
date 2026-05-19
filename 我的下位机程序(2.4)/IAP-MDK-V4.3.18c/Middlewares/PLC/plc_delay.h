#ifndef __PLC_DELAY_H__
#define __PLC_DELAY_H__

#include "plc_type.h"

// É¨Ãè
void plc_delay_scan(void);

// Âß¼­¿ØÖÆ
void plc_delay_flow(uint8_t id, uint32_t time_dly,\
	uint8_t feedback, uint8_t modbus_port_id,\
	PLC_TYPE_FLOW_INFO *p_flow_info, uint8_t *p_flow_pos, uint8_t flow_mask);

#endif
