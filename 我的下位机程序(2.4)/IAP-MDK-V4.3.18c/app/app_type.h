#ifndef __APP_TYPE_H__
#define __APP_TYPE_H__

#include <stdint.h>

/*
* 通信数据存储空间
*/
#define APP_USER_COMM_BUF_ROW_SIZE	28
#define APP_USER_COMM_BUF_COL_SIZE	16
#define APP_USER_COMM_BUF_SIZE			(APP_USER_COMM_BUF_ROW_SIZE*APP_USER_COMM_BUF_COL_SIZE)
extern uint8_t usAppUserCommBuf[APP_USER_COMM_BUF_ROW_SIZE][APP_USER_COMM_BUF_COL_SIZE];


#endif
