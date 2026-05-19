#include "plc_eep.h"

typedef enum{
	CONFIG_PLC_EEP_MODE_WRITE = 0,
	CONFIG_PLC_EEP_MODE_READ,
}PLC_EEP_MODE;

#define CONFIG_PLC_EEP_WRITE_CYCLE	10

typedef struct
{	
	// 端口地址
	I2C_HandleTypeDef *hi2c;
	
	// 设备地址
	uint8_t dev_write_address;
	uint8_t dev_read_address;
	
	//流程端口
	uint8_t            feedback;
	PLC_TYPE_FLOW_INFO *p_flow_info;
	
	//通信端口
	uint8_t modbus_port_id;
	
	//时间标记
	uint32_t time_tick;
}PLC_EEP_PORT;

PLC_EEP_PORT g_plc_eep_port[CONFIG_PLC_PLC_PORT_COUNT];

/*
************************************************************************************
* 初始化
************************************************************************************
*/
void plc_eep_init(uint8_t id, uint8_t dev_addr, I2C_HandleTypeDef *hi2c)
{
	g_plc_eep_port[id].hi2c  = hi2c;
	
	g_plc_eep_port[id].dev_write_address = 0xA0 | CONFIG_PLC_EEP_MODE_WRITE;
	g_plc_eep_port[id].dev_read_address  = 0xA0 | CONFIG_PLC_EEP_MODE_READ;
}

/*
************************************************************************************
* 读内容
************************************************************************************
*/
uint8_t plc_eep_read_value(uint8_t id, uint16_t address, uint8_t size, uint32_t *value)
{
	PLC_EEP_PORT *p_eep_port = g_plc_eep_port + id;
	uint8_t  i, buf[4];
	
	if(p_eep_port->time_tick > HAL_GetTick())
			return FALSE;
	
	if(HAL_I2C_Mem_Read(p_eep_port->hi2c, p_eep_port->dev_read_address, address, I2C_MEMADD_SIZE_16BIT, buf, size, 100) != HAL_OK)
		return FALSE;
	
	*value = 0;
	for(i = 0; i < size; i++){	//高位在前
		*value = *value<<8;
		*value += buf[i];
	}
	
	return TRUE;
}	

uint8_t plc_eep_read_buf(uint8_t id, uint16_t address, uint8_t size, uint8_t *buf)
{
	PLC_EEP_PORT *p_eep_port = g_plc_eep_port + id;
	
	if(p_eep_port->time_tick > HAL_GetTick())
			return FALSE;
	
	if(HAL_I2C_Mem_Read(p_eep_port->hi2c, p_eep_port->dev_read_address, address, I2C_MEMADD_SIZE_16BIT, buf, size, 100) != HAL_OK)
		return FALSE;
	else	
		return TRUE;
}	

/*
************************************************************************************
* 写内容
************************************************************************************
*/
uint8_t plc_eep_write_value(uint8_t id, uint16_t address, uint8_t size, uint32_t value)
{
	PLC_EEP_PORT *p_eep_port = g_plc_eep_port + id;
	int8_t  i;
	uint8_t buf[4];
	
	if(p_eep_port->time_tick > HAL_GetTick())
			return FALSE;
	
	for(i = (size - 1); i >=0; i--){	//高位在前
		buf[i] = value & 0xFF;
		value = value>>8;
	}
	
	if(HAL_I2C_Mem_Write(p_eep_port->hi2c, p_eep_port->dev_write_address, address, I2C_MEMADD_SIZE_16BIT, buf, size, 100) != HAL_OK){
		return FALSE;
	}else{
		p_eep_port->time_tick = HAL_GetTick() + CONFIG_PLC_EEP_WRITE_CYCLE;
		return TRUE;
	}
}	

uint8_t plc_eep_write_buf(uint8_t id, uint16_t address, uint8_t size, uint8_t *buf)
{
	PLC_EEP_PORT *p_eep_port = g_plc_eep_port + id;
	
	if(p_eep_port->time_tick > HAL_GetTick())
			return FALSE;
	
	if(HAL_I2C_Mem_Write(p_eep_port->hi2c, p_eep_port->dev_write_address, address, I2C_MEMADD_SIZE_16BIT, buf, size, 100) != HAL_OK){
		return FALSE;
	}else{
		p_eep_port->time_tick = HAL_GetTick() + CONFIG_PLC_EEP_WRITE_CYCLE;
		return TRUE;
	}
}

/*
************************************************************************************
* modbus
************************************************************************************
*/
int8_t plc_eep_modbus_write(uint8_t address, uint8_t *buf, uint8_t pos, uint8_t mask)
{
	uint8_t i, eep_buf[CONFIG_PLC_EEP_MODBUS_LEN];
	
	for(i = 0; i < CONFIG_PLC_EEP_MODBUS_LEN; i++){
		eep_buf[i] = buf[(pos + i) & mask];
	}
	
	plc_eep_write_buf(0, address, CONFIG_PLC_EEP_MODBUS_LEN, eep_buf);
	
	return CONFIG_PLC_EEP_MODBUS_LEN;
}

int8_t plc_eep_modbus_read(uint8_t address, uint8_t *buf, uint8_t pos, uint8_t mask)
{
	uint8_t i, eep_buf[CONFIG_PLC_EEP_MODBUS_LEN];

	plc_eep_read_buf(0, address, CONFIG_PLC_EEP_MODBUS_LEN, eep_buf);
	
	for(i = 0; i < CONFIG_PLC_EEP_MODBUS_LEN; i++){
		buf[(pos + i) & mask] = eep_buf[i];
	}
	
	return CONFIG_PLC_EEP_MODBUS_LEN;
}



