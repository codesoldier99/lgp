#ifndef __PLC_TEST_H__
#define __PLC_TEST_H__

#include "plc_type.h"
#include "plc_cfg.h"

#define CONFIG_PLC_TEST_SCAN	



#ifdef CONFIG_PLC_TEST_SCAN
	void plc_flow_test_scan(void);
	uint8_t plc_motor_pos_flag(uint8_t flag1);
	void Close_read_camera(void);
	void finishRun(uint8_t id);
	void finishRst(uint8_t id);
#endif

#endif
