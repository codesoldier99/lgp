/*
 * FreeModbus Libary: BARE Port
 * Copyright (C) 2006 Christian Walter <wolti@sil.at>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 * File: $Id: porttimer.c,v 1.1 2006/08/22 21:35:13 wolti Exp $
 */

#include "mbport.h"

inline void vMBPortTimersDisable(MB_VAR_TypeDef *pMbVarStruct)
{
	pMbVarStruct->timer_tick = 0;
}

inline void vMBPortTimersEnable(MB_VAR_TypeDef *pMbVarStruct)
{
	pMbVarStruct->timer_tick = HAL_GetTick() + pMbVarStruct->timer_value;
}

inline void vMBPortTimersFrame(MB_PORT_TypeDef *pMbPortStruct)
{
	MB_FUNC_TypeDef *p_func_struct = &pMbPortStruct->func_struct;
	MB_VAR_TypeDef  *p_var_struct  = &pMbPortStruct->var_struct;
	
	if(HAL_GetTick() > p_var_struct->timer_tick && p_var_struct->timer_tick != 0){
		p_func_struct->pxMBPortCBTimerExpired(p_var_struct);
		p_var_struct->timer_tick = 0;
	}
}
