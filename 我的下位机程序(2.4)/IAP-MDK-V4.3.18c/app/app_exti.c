#include "app_exti.h"
#include "stm32f1xx_hal.h"
#include "plc_input.h"

/* 中断回调 */
void HAL_GPIO_EXTI_Callback(uint16_t GPIO_Pin)
{
	//NPN输入管脚信号
	if(GPIO_Pin == GPIO_PIN_0){
		plc_input_write(0, HAL_GPIO_ReadPin(GPIOD, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_1){
		plc_input_write(1, HAL_GPIO_ReadPin(GPIOD, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_2){
		plc_input_write(2, HAL_GPIO_ReadPin(GPIOG, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_3){
		plc_input_write(3, HAL_GPIO_ReadPin(GPIOD, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_4){
		plc_input_write(4, HAL_GPIO_ReadPin(GPIOD, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_5){
		plc_input_write(5, HAL_GPIO_ReadPin(GPIOD, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_6){
		plc_input_write(6, HAL_GPIO_ReadPin(GPIOD, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_7){
		plc_input_write(7, HAL_GPIO_ReadPin(GPIOD, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_8){
		plc_input_write(8, HAL_GPIO_ReadPin(GPIOA, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_9){
		plc_input_write(9, HAL_GPIO_ReadPin(GPIOG, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_10){
		plc_input_write(10, HAL_GPIO_ReadPin(GPIOG, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_11){
		plc_input_write(11, HAL_GPIO_ReadPin(GPIOG, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_12){
		plc_input_write(12, HAL_GPIO_ReadPin(GPIOG, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_13){
		plc_input_write(13, HAL_GPIO_ReadPin(GPIOG, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_14){
		plc_input_write(14, HAL_GPIO_ReadPin(GPIOG, GPIO_Pin));
	}else if(GPIO_Pin == GPIO_PIN_15){
		plc_input_write(15, HAL_GPIO_ReadPin(GPIOG, GPIO_Pin));
	}
}

