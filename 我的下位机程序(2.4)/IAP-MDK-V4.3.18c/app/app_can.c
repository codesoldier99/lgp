#include "app_can.h"
#include "plc_can.h"

/*
接收数据
*/
void HAL_CAN_RxFifo1MsgPendingCallback(CAN_HandleTypeDef *hcan)
{
	if(hcan->Instance == CAN1){
		plc_can_receive(hcan);
	}
}

