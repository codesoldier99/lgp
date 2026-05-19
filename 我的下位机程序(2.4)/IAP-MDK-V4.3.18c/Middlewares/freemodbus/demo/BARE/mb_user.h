
#ifndef __MB_USER__
#define __MB_USER__

#include "mb_type.h"

typedef enum
{
	MODBUS_PORT_UART1 = 0,
	MODBUS_PORT_UART6,
	MODBUS_PORT_TCP,
	MODBUS_PORT_COUNT
}eMBPortCode;

extern MB_PORT_TypeDef MB_PORT_Struct[MODBUS_PORT_COUNT];

/**
* modbus»ŒŒÒ
*/
void MB_USER_Task(void *pvParameter);

#endif

