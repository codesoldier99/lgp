#include "plc_modbus.h"

#include "plc_input.h"
#include "plc_switch.h"
#include "plc_bridge.h"
#include "plc_beep.h"
#include "plc_motor.h"
#include "plc_eep.h"

/*
***********************************************************************************
* 合法性
***********************************************************************************
*/
typedef enum{
	CONFIG_PLC_MB_INPUT = 0,
	CONFIG_PLC_MB_SWITCH,
	CONFIG_PLC_MB_DCMOTOR,
	CONFIG_PLC_MB_BEEP,
	CONFIG_PLC_MB_STEPMOTOR,
	CONFIG_PLC_MB_EEPROM,
}PLC_MB_PORT;	
uint8_t plc_modbus_adjust(uint16_t address, uint16_t count, uint8_t *id, uint8_t *function)
{
	if(address >= CONFIG_PLC_MB_ADDRESS_INPUT && address < CONFIG_PLC_MB_ADDRESS_SWITCH)
	{
		if((address - CONFIG_PLC_MB_ADDRESS_INPUT)%CONFIG_PLC_INPUT_MODBUS_LEN != 0 || count != CONFIG_PLC_INPUT_MODBUS_LEN/2)
			return FALSE;
		
		*id = (address - CONFIG_PLC_MB_ADDRESS_INPUT)/CONFIG_PLC_INPUT_MODBUS_LEN;
		*function = CONFIG_PLC_MB_INPUT;
	}else if(address >= CONFIG_PLC_MB_ADDRESS_SWITCH && address < CONFIG_PLC_MB_ADDRESS_BRIDGE)
	{
		if((address - CONFIG_PLC_MB_ADDRESS_SWITCH)%CONFIG_PLC_SWITCH_MODBUS_LEN != 0 || count != CONFIG_PLC_SWITCH_MODBUS_LEN/2)
			return FALSE;
		
		*id = (address - CONFIG_PLC_MB_ADDRESS_SWITCH)/CONFIG_PLC_SWITCH_MODBUS_LEN;
		*function = CONFIG_PLC_MB_SWITCH;
	}else if(address >= CONFIG_PLC_MB_ADDRESS_BRIDGE && address < CONFIG_PLC_MB_ADDRESS_BEEP)
	{
		if((address - CONFIG_PLC_MB_ADDRESS_BRIDGE)%CONFIG_PLC_BRIDGE_MODBUS_LEN != 0 || count != CONFIG_PLC_BRIDGE_MODBUS_LEN/2)
			return FALSE;
		
		*id = (address - CONFIG_PLC_MB_ADDRESS_BRIDGE)/CONFIG_PLC_BRIDGE_MODBUS_LEN;
		*function = CONFIG_PLC_MB_DCMOTOR;
	}else if(address >= CONFIG_PLC_MB_ADDRESS_BEEP && address < CONFIG_PLC_MB_ADDRESS_STEPMOTOR){
		if((address - CONFIG_PLC_MB_ADDRESS_BEEP)%CONFIG_PLC_BEEP_MODBUS_LEN != 0 || count != CONFIG_PLC_BEEP_MODBUS_LEN/2)
			return FALSE;
		
		*id = (address - CONFIG_PLC_MB_ADDRESS_BEEP)/CONFIG_PLC_BEEP_MODBUS_LEN;
		*function = CONFIG_PLC_MB_BEEP;
	}else if(address >= CONFIG_PLC_MB_ADDRESS_STEPMOTOR && address < CONFIG_PLC_MB_ADDRESS_EEPROM){
		if((address - CONFIG_PLC_MB_ADDRESS_STEPMOTOR)%CONFIG_PLC_MOTOR_MODBUS_LEN != 0 || count != CONFIG_PLC_MOTOR_MODBUS_LEN/2)
			return FALSE;
		
		*id = (address - CONFIG_PLC_MB_ADDRESS_STEPMOTOR)/CONFIG_PLC_MOTOR_MODBUS_LEN;
		*function = CONFIG_PLC_MB_STEPMOTOR;
	}else{
		*id = (address - CONFIG_PLC_MB_ADDRESS_EEPROM);
		*function = CONFIG_PLC_MB_EEPROM;
	}
	
	return TRUE;
}

/*
***********************************************************************************
* 读内容
***********************************************************************************
*/
uint8_t plc_modbus_read(uint8_t *txdbuf, uint8_t *pos, uint8_t mask, uint16_t address, uint16_t count)
{
	uint8_t id, function;
	
	if(plc_modbus_adjust(address, count, &id, &function) == FALSE){
		return FALSE;
	}
	
	switch(function)
	{
		case CONFIG_PLC_MB_INPUT:			*pos = (*pos + plc_input_modbus_read(id, txdbuf, *pos, mask)) & mask; 		break;
		case CONFIG_PLC_MB_SWITCH: 		*pos = (*pos + plc_switch_modbus_read(id, txdbuf, *pos, mask)) & mask; 		break;
		case CONFIG_PLC_MB_DCMOTOR: 	*pos = (*pos + plc_bridge_modbus_read(id, txdbuf, *pos, mask)) & mask;		break;
		case CONFIG_PLC_MB_BEEP: 			*pos = (*pos + plc_beep_modbus_read(id, txdbuf, *pos, mask)) & mask; 			break;
		case CONFIG_PLC_MB_STEPMOTOR: *pos = (*pos + plc_motor_modbus_read(id, txdbuf, *pos, mask)) & mask; break;
		case CONFIG_PLC_MB_EEPROM:    *pos = (*pos + plc_eep_modbus_read(id, txdbuf, *pos, mask)) & mask;       break;
		default: break;
	}
	
	return TRUE;
}

/*
***********************************************************************************
* 写内容
***********************************************************************************
*/
uint8_t plc_modbus_write(uint8_t *rxdbuf, uint8_t *pos, uint8_t mask, uint16_t address, uint16_t count)
{
	uint8_t id, function;
	
	if(plc_modbus_adjust(address, count, &id, &function) == FALSE){
		return FALSE;
	}
	
	switch(function)
	{
		case CONFIG_PLC_MB_INPUT:			*pos = (*pos + plc_input_modbus_write(id, rxdbuf, *pos, mask)) & mask; 		break;
		case CONFIG_PLC_MB_SWITCH: 		*pos = (*pos + plc_switch_modbus_write(id, rxdbuf, *pos, mask)) & mask; 	break;
		case CONFIG_PLC_MB_DCMOTOR: 	*pos = (*pos + plc_bridge_modbus_write(id, rxdbuf, *pos, mask)) & mask;	break;
		case CONFIG_PLC_MB_BEEP: 			*pos = (*pos + plc_beep_modbus_write(id, rxdbuf, *pos, mask)) & mask; 		break;
		case CONFIG_PLC_MB_STEPMOTOR: *pos = (*pos + plc_motor_modbus_write(id, rxdbuf, *pos, mask)) & mask;break;
		case CONFIG_PLC_MB_EEPROM:    *pos = (*pos + plc_eep_modbus_write(id, rxdbuf, *pos, mask)) & mask;			break;
		default: break;
	}
	
	return TRUE;
}
