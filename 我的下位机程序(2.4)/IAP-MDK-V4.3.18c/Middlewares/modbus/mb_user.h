#ifndef __MB_USER_H__
#define __MB_USER_H__

#include "mb_port.h"
#include "mb_type.h"
#include "mb_serial.h"

#include "main.h"

// 端口初始化
void mb_user_init(void);

// 解析
uint8_t mb_user_poll(void);

#endif

