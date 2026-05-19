#ifndef __PLC_TYPE_H__
#define __PLC_TYPE_H__

#include "stm32f1xx_hal.h"
#include "main.h"

#include "plc_cfg.h"

#include "mb_rtu.h"

#ifndef TRUE
	#define TRUE	1
#endif

#ifndef FALSE
	#define FALSE	0
#endif

typedef struct{
	uint8_t stat;
	uint8_t port;
	uint8_t id;
}PLC_TYPE_FLOW_INFO; 

typedef enum{
	CONFIG_PLC_FEEDBACK_FREE = 0,
	CONFIG_PLC_FEEDBACK_UART,
	CONFIG_PLC_FEEDBACK_FLOW,
}PLC_FEEDBACK_PORT;

#endif
