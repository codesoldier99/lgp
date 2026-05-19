#include "app_tim.h"

#include "plc_motor.h"

void HAL_TIM_PeriodElapsedCallback(TIM_HandleTypeDef *htim)
{
	if(htim->Instance == TIM5){
		plc_motor_pul_action(0);
	}else if(htim->Instance == TIM6){
		plc_motor_pul_action(1);
	}else if(htim->Instance == TIM7){
		plc_motor_pul_action(2);
	}else if(htim->Instance == TIM8){
		plc_motor_pul_action(3);
	}else if(htim->Instance == TIM9){
		plc_motor_pul_action(4);
	}else if(htim->Instance == TIM10){
		plc_motor_pul_action(5);
	}else if(htim->Instance == TIM11){
		plc_motor_pul_action(6);
	}else if(htim->Instance == TIM12){
		plc_motor_pul_action(7);
	}else if(htim->Instance == TIM13){
		plc_motor_pul_action(8);
	}else if(htim->Instance == TIM14){
		plc_motor_pul_action(9);
	}
}

