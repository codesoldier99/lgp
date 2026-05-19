#include "mb_timer.h"

/*
*****************************************************************
* 时钟刷新
*****************************************************************
*/
inline void mb_timer_rcv_update(MB_VAR_PORT *p_mb_var_port)
{
	p_mb_var_port->frame_rcv_tick = HAL_GetTick() + p_mb_var_port->frame_rcv_value;
}

inline void mb_timer_snd_update(MB_VAR_PORT *p_mb_var_port)
{
	p_mb_var_port->frame_snd_tick = HAL_GetTick() + p_mb_var_port->frame_snd_value;
}


/*
*****************************************************************
* 时钟失效
*****************************************************************
*/
inline void mb_timer_rcv_disable(MB_VAR_PORT *p_mb_var_port)
{
	p_mb_var_port->frame_rcv_tick = 0;
}

/*
*****************************************************************
* 超时判断
*****************************************************************
*/
inline uint8_t mb_timer_rcv_outtime(MB_VAR_PORT *p_mb_var_port)
{
	if((HAL_GetTick() > p_mb_var_port->frame_rcv_tick) && p_mb_var_port->frame_rcv_tick != 0){
		return TRUE;
	}else{
		return FALSE;
	}
}

inline uint8_t mb_timer_snd_outtime(MB_VAR_PORT *p_mb_var_port)
{
	if((HAL_GetTick() > p_mb_var_port->frame_snd_tick)){
		return TRUE;
	}else{
		return FALSE;
	}
}
