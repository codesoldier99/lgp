#include "mb_port.h"

#include "mb_serial.h"
#include "mb_err.h"

#include "mb_rtu.h"
#include "mb_ascii.h"
#include "mb_tcp.h"

#include "mb_func_holding.h"

static xMBFunctionHandler xFuncHandlers[MB_FUNC_HANDLERS_MAX] = {
	#if MB_FUNC_OTHER_REP_SLAVEID_ENABLED > 0
		{MB_FUNC_OTHER_REPORT_SLAVEID, eMBFuncReportSlaveID, NULL},
	#endif
	#if MB_FUNC_READ_INPUT_ENABLED > 0
		{MB_FUNC_READ_INPUT_REGISTER, eMBFuncReadInputRegister, NULL},
	#endif
	#if MB_FUNC_READ_HOLDING_ENABLED > 0
		{MB_FUNC_READ_HOLDING_REGISTER, eMBFuncReadHoldingRegisterSlave, eMBFuncReadHoldingRegisterMaster},
	#endif
	#if MB_FUNC_WRITE_MULTIPLE_HOLDING_ENABLED > 0
		{MB_FUNC_WRITE_MULTIPLE_REGISTERS, eMBFuncWriteMultipleHoldingRegisterSlave, eMBFuncWriteMultipleHoldingRegisterMaster},
	#endif
	#if MB_FUNC_WRITE_HOLDING_ENABLED > 0
		{MB_FUNC_WRITE_REGISTER, eMBFuncWriteHoldingRegisterSlave, eMBFuncWriteHoldingRegisterMaster},
	#endif
	#if MB_FUNC_READWRITE_HOLDING_ENABLED > 0
		{MB_FUNC_READWRITE_MULTIPLE_REGISTERS, eMBFuncReadWriteMultipleHoldingRegisterSlave, eMBFuncReadWriteMultipleHoldingRegisterMaster},
	#endif
	#if MB_FUNC_READ_COILS_ENABLED > 0
		{MB_FUNC_READ_COILS, eMBFuncReadCoils, NULL},
	#endif
	#if MB_FUNC_WRITE_COIL_ENABLED > 0
		{MB_FUNC_WRITE_SINGLE_COIL, eMBFuncWriteCoil, NULL},
	#endif
	#if MB_FUNC_WRITE_MULTIPLE_COILS_ENABLED > 0
		{MB_FUNC_WRITE_MULTIPLE_COILS, eMBFuncWriteMultipleCoils, NULL},
	#endif
	#if MB_FUNC_READ_DISCRETE_INPUTS_ENABLED > 0
		{MB_FUNC_READ_DISCRETE_INPUTS, eMBFuncReadDiscreteInputs, NULL},
	#endif
};




/*
*****************************************************************
* 端口初始化
*****************************************************************
*/
eMBErrorCode mb_port_init(uint8_t id, eMBCommMode eCommMode, eMBMode eMode,\
	uint8_t ucSlaveAddress, uint16_t ucPort, uint32_t ulBaudRate, eMBParity eParity)
{
	eMBErrorCode eStatus = MB_ENOERR;
	
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
	
	p_mb_var_port->comm_mode = eCommMode;
	if((ucSlaveAddress == MB_ADDRESS_BROADCAST) || (ucSlaveAddress < MB_ADDRESS_MIN) || (ucSlaveAddress > MB_ADDRESS_MAX))
	{
		eStatus = MB_EINVAL;
	}
	else
	{	
		switch ( eMode )
		{
			#if MB_RTU_ENABLED > 0
				case MB_RTU:
					p_mb_var_port->mode  = MB_RTU;
					p_mb_var_port->poll_event = MB_EV_READY;
				
					p_mb_var_port->mb_receive       = mb_rtu_receive;
					p_mb_var_port->mb_rev_adjust    = mb_rtu_rev_adjust;
					p_mb_var_port->mb_snd_frame     = mb_rtu_snd_frame;
					p_mb_var_port->mb_segmentation  = mb_rtu_segmentation;
					p_mb_var_port->mb_frame_conform = mb_rtu_frame_conform;
					
					eStatus = mb_rtu_init(id, ucSlaveAddress, ucPort, ulBaudRate, eParity);
				break;
			#endif
			
			#if MB_ASCII_ENABLED > 0
        case MB_ASCII:	
					p_mb_var_port->mode = MB_ASCII;
					p_mb_var_port->poll_event = MB_EV_READY;
				
					p_mb_var_port->mb_receive       = mb_ascii_receive;
					p_mb_var_port->mb_rev_adjust    = mb_ascii_rev_adjust;
					p_mb_var_port->mb_snd_frame     = mb_ascii_snd_frame;
					p_mb_var_port->mb_segmentation  = mb_ascii_segmentation;
					p_mb_var_port->mb_frame_conform = mb_ascii_frame_conform;
					
					eStatus = mb_ascii_init(id, ucSlaveAddress, ucPort, ulBaudRate, eParity);
				break;
			#endif
				
			#if MB_TCP_ENABLED > 0
				case MB_TCP:
					p_mb_var_port->mode = MB_TCP;
				
					p_mb_var_port->poll_event = MB_EV_READY;
				
					p_mb_var_port->mb_receive       = mb_tcp_receive;
					p_mb_var_port->mb_rev_adjust    = mb_tcp_rev_adjust;
					p_mb_var_port->mb_snd_frame     = mb_tcp_snd_frame;
					p_mb_var_port->mb_segmentation  = mb_tcp_segmentation;
					p_mb_var_port->mb_frame_conform = mb_tcp_frame_conform;
					
					eStatus = mb_tcp_init(id, ucSlaveAddress, ucPort);
				break;
			#endif
		}
	}
	
	 return eStatus;
}

/*
*****************************************************************
* 解析
*****************************************************************
*/
eMBErrorCode mb_port_poll( uint8_t id )
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
	eMBErrorCode eStatus = MB_ENOERR;
	
	eMBException eException;
	int          i;
	
/*
扫描接收
*/
	if(id == CONFIG_MB_PORT_SPI2){
//		mb_tcp_receive_poll(CONFIG_MB_PORT_SPI2);
	}
/*
解析
*/	
	switch(p_mb_var_port->poll_event)
	{
		case MB_EV_READY:
			if(p_mb_var_port->mb_rev_adjust(id) == TRUE){
				p_mb_var_port->poll_event = MB_EV_FRAME_SEGMENTATION;
			}
		break;
			
		case MB_EV_FRAME_SEGMENTATION:
			if(p_mb_var_port->mb_segmentation( id, &p_mb_var_port->cur_frame_address, &p_mb_var_port->cur_rxd_frame_pos, &p_mb_var_port->cur_rxd_frame_len ) == MB_ENOERR){
				if( p_mb_var_port->cur_frame_address == p_mb_var_port->address || p_mb_var_port->cur_frame_address == MB_ADDRESS_BROADCAST ){
					if(p_mb_var_port->comm_mode == MB_SLAVE)
						p_mb_var_port->poll_event = MB_EV_EXECUTE_SLAVE;
					else
						p_mb_var_port->poll_event = MB_EV_EXECUTE_MASTER;
				}
			}else{
				p_mb_var_port->poll_event = MB_EV_READY;
			}
		break;
			
		case MB_EV_EXECUTE_SLAVE:
			eException = MB_EX_ILLEGAL_FUNCTION;
			p_mb_var_port->ucFunctionCode = p_mb_var_port->rxdbuffer[CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->cur_rxd_frame_pos, MB_PDU_FUNC_OFF)];
			for( i = 0; i < MB_FUNC_HANDLERS_MAX; i++ ){
				if( xFuncHandlers[i].ucFunctionCode == 0 ){	/* No more function handlers registered. Abort. */
						break;
				}else if( xFuncHandlers[i].ucFunctionCode == p_mb_var_port->ucFunctionCode ){
						eException = xFuncHandlers[i].pxHandlerSlave( p_mb_var_port );
						break;
				}
			}

			if( p_mb_var_port->cur_frame_address != MB_ADDRESS_BROADCAST ){	/* If the request was not sent to the broadcast address we return a reply. */
				if( eException != MB_EX_NONE ){
					mb_err_frame(p_mb_var_port, eException);
				}
				if( (p_mb_var_port->mode == MB_ASCII ) && MB_ASCII_TIMEOUT_WAIT_BEFORE_SEND_MS ){
					p_mb_var_port->frame_snd_tick = HAL_GetTick() + MB_ASCII_TIMEOUT_WAIT_BEFORE_SEND_MS;
				}
				p_mb_var_port->mb_frame_conform(id);	// 完善帧内容
			}
			p_mb_var_port->poll_event = MB_EV_READY;
		break;
			
		case MB_EV_EXECUTE_MASTER:
			p_mb_var_port->ucFunctionCode = p_mb_var_port->rxdbuffer[CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->cur_rxd_frame_pos, MB_PDU_FUNC_OFF)];
		
			for( i = 0; i < MB_FUNC_HANDLERS_MAX; i++ ){
				if( xFuncHandlers[i].ucFunctionCode == p_mb_var_port->ucFunctionCode ){
						eException = xFuncHandlers[i].pxHandlerMaster( p_mb_var_port );
						break;
				}
			}
			
			p_mb_var_port->poll_event = MB_EV_READY;
		break;
	}
	
/*
发送
*/	
	p_mb_var_port->mb_snd_frame(id);
		
	return eStatus;
}

