#include "app_user.h"
#include "app_config.h"
#include "app_type.h"

//#include "FreeRTOS.h"
//#include "semphr.h"
//#include "task.h"

//#include "amc_user.h"
//#include "mb_user.h"

/*
* 通信数据存储空间
*/
uint8_t usAppUserCommBuf[APP_USER_COMM_BUF_ROW_SIZE][APP_USER_COMM_BUF_COL_SIZE] = {1,2,3,4,5,6,7,8,9,10};

/**
* app任务
*
*/

void APP_USER_CreateTask(void)
{	
	//amc任务
//	xTaskCreate(AMC_USER_Task, "AMC_USER_Task", 1024, NULL, 1, NULL);
	
	//modbus任务
//	xTaskCreate(MB_USER_Task, "MB_USER_Task", 1024, NULL, 2, NULL);
	
	//mqtt任务
//	xTaskCreate(MQTT_USER_Task, "MQTT_USER_Task", 2048, NULL, 3, NULL);
	
	//canopen任务
//	xTaskCreate(CO_USER_Task, "co_user_task", 1024, NULL, 4, NULL);
}

