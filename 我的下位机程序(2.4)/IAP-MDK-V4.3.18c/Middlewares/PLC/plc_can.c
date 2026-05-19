#include "plc_can.h"

typedef struct{
	CAN_TxHeaderTypeDef tx_header;
	CAN_RxHeaderTypeDef rx_header;
	
	uint8_t  rx_buf[8];
	uint8_t  tx_buf[8];
}PLC_CAN_PORT;

PLC_CAN_PORT g_plc_can_port;

typedef enum{
	CONFIG_PLC_CAN_ID_1 = 0x001,  
	CONFIG_PLC_CAN_ID_2 = 0x002,
	CONFIG_PLC_CAN_ID_3 = 0x003,
	CONFIG_PLC_CAN_ID_4 = 0x004,
}PLC_CNA_ID_PORT;

/*
**************************************************************************************
* 初始化
**************************************************************************************
*/

uint8_t plc_can_init(CAN_HandleTypeDef *hcan)
{  
	CAN_FilterTypeDef  sFilterConfig;  
	HAL_StatusTypeDef  HAL_Status;  
	
	g_plc_can_port.tx_header.IDE = CAN_ID_STD;
	g_plc_can_port.tx_header.RTR = CAN_RTR_DATA;  
	
	sFilterConfig.FilterBank           = 0;
	sFilterConfig.FilterMode           = CAN_FILTERMODE_IDLIST;
	sFilterConfig.FilterScale          = CAN_FILTERSCALE_16BIT;      
	
	sFilterConfig.FilterIdHigh         = CONFIG_PLC_CAN_ID_1<<5;
	sFilterConfig.FilterIdLow          = CONFIG_PLC_CAN_ID_2<<5;      
	
	sFilterConfig.FilterMaskIdHigh     = CONFIG_PLC_CAN_ID_3<<5;  
	sFilterConfig.FilterMaskIdLow      = CONFIG_PLC_CAN_ID_4<<5;   
	
	sFilterConfig.FilterFIFOAssignment = CAN_RX_FIFO1;
	sFilterConfig.FilterActivation     = ENABLE;
	sFilterConfig.SlaveStartFilterBank = 0;   
	HAL_Status=HAL_CAN_ConfigFilter(hcan, &sFilterConfig);  
	
	HAL_Status=HAL_CAN_Start(hcan);
	if(HAL_Status != HAL_OK){  
		return FALSE; 
	}
	
	HAL_Status=HAL_CAN_ActivateNotification(hcan, CAN_IT_RX_FIFO1_MSG_PENDING);  
	if(HAL_Status!=HAL_OK){  
		return FALSE; 
	}
	
	return TRUE;
}

/*
**************************************************************************************
* 接收数据
**************************************************************************************
*/
inline void plc_can_receive(CAN_HandleTypeDef *hcan)
{
	HAL_CAN_GetRxMessage(hcan, CAN_RX_FIFO1, &g_plc_can_port.rx_header, g_plc_can_port.rx_buf);
}

/*
**************************************************************************************
* 发送数据
**************************************************************************************
*/
void plc_can_send(CAN_HandleTypeDef *hcan, uint32_t std_id, uint8_t *buf, uint8_t len)
{
	g_plc_can_port.tx_header.StdId = std_id;
  g_plc_can_port.tx_header.DLC   = len;

	HAL_CAN_AddTxMessage(hcan, &g_plc_can_port.tx_header, buf, (uint32_t*)CAN_TX_MAILBOX1);
}
