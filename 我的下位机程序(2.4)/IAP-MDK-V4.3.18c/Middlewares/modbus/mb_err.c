#include "mb_err.h"


/*
***********************************************************************************************
* ´íÎó·´À¡Ö¡
***********************************************************************************************
*/
void mb_err_frame(MB_VAR_PORT *p_mb_var_port, eMBException eException)
{
	p_mb_var_port->cur_txd_frame_pos = p_mb_var_port->txdbufpos;
	if(p_mb_var_port->mode == MB_RTU || p_mb_var_port->mode == MB_ASCII)
		p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	else
		p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 7);
		
	p_mb_var_port->cur_txd_frame_len = 0;
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = ( uint8_t )( p_mb_var_port->ucFunctionCode | MB_FUNC_ERROR );
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	
	p_mb_var_port->cur_txd_frame_len++;
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = eException;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
}

