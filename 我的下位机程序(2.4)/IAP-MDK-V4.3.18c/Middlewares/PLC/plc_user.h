#ifndef __PLC_USER_H__
#define __PLC_USER_H__

#include "stdint.h"

// ¶Ë¿Ú³õÊ¼»¯
void plc_user_init(void);

// É¨Ãè
uint8_t plc_user_scan(uint8_t enable);
	
#endif
