#include "stm32f1xx_hal.h"

#include "eth_spi.h"
#include "main.h"

/*
******************************************************************
* 函数名  : 
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  : 
* 说明    : 
******************************************************************
*/
void ETH_SPI_SetCS(uint8_t val)
{
	if (val == LOW) {
		HAL_GPIO_WritePin(GPIOB, GPIO_PIN_12, GPIO_PIN_RESET);
	}
	else if (val == HIGH){
   	HAL_GPIO_WritePin(GPIOB, GPIO_PIN_12, GPIO_PIN_SET);
	}
}

/*
******************************************************************
* 函数名  : 
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  : 
* 说明    : 
******************************************************************
*/
void ETH_SPI_SetRst(uint8_t val)
{
	if (val == LOW) {
		HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_RESET);
	}
	else if (val == HIGH){
   	HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_SET);
	}
}

/*
******************************************************************
* 函数名  : 
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  : 
* 说明    : 
******************************************************************
*/
inline void EHT_SPI_WriteByte1(uint8_t dat)
{
	HAL_StatusTypeDef status = HAL_ERROR;
	
	while(status != HAL_OK){
		status = HAL_SPI_Transmit(&hspi2, &dat, 1, 1);
	}
}

void EHT_SPI_WriteByte2(uint16_t dat)
{
	uint8_t buf[2];	
	HAL_StatusTypeDef status = HAL_ERROR;
	
	buf[0] = dat/256;
	buf[1] = dat%256;
	
	while(status != HAL_OK){
		status = HAL_SPI_Transmit(&hspi2, buf, 2, 1);
	}
}

/*
******************************************************************
* 函数名  : 
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  : 
* 说明    : 
******************************************************************
*/
uint8_t EHT_SPI_ReadByte1(void)
{
	uint8_t rxd;
	
	HAL_StatusTypeDef status = HAL_ERROR;
	
	while(status != HAL_OK){
		status = HAL_SPI_Receive(&hspi2, &rxd, 1, 1);
	}
	
	return rxd;
}

/*
******************************************************************
* 函数名  : 
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  : 
* 说明    : 
******************************************************************
*/
inline uint32_t EHT_GET_TICK(void){
	return HAL_GetTick();
}
