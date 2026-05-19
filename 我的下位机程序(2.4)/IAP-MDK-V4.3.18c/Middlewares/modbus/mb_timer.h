#ifndef __MB_TIMER_H__
#define __MB_TIMER_H__

#include "mb_type.h"

// 时钟刷新
void mb_timer_rcv_update(MB_VAR_PORT *p_mb_var_port);
void mb_timer_snd_update(MB_VAR_PORT *p_mb_var_port);

// 时钟失效
void mb_timer_rcv_disable(MB_VAR_PORT *p_mb_var_port);
	
// 超时判断
uint8_t mb_timer_rcv_outtime(MB_VAR_PORT *p_mb_var_port);
uint8_t mb_timer_snd_outtime(MB_VAR_PORT *p_mb_var_port);
	
#endif
