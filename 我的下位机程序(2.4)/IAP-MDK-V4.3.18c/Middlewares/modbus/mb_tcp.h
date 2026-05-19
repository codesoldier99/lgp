#ifndef __MB_TCP_H__
#define __MB_TCP_H__

#include "mb_type.h"

// 初始化
eMBErrorCode mb_tcp_init(uint8_t id, uint8_t ucSlaveAddress, uint16_t ucPort);

// 接收数据
void mb_tcp_receive(uint8_t id, uint8_t context);

// 发送帧数据
uint8_t mb_tcp_snd_frame(uint8_t id);

// 帧判断
uint8_t mb_tcp_rev_adjust(uint8_t id);

// 完善帧内容
void mb_tcp_frame_conform(uint8_t id);

// 帧分段（从地址 + 帧内容 + 帧长度）
uint8_t mb_tcp_segmentation( uint8_t id, uint8_t *cur_frame_address, uint8_t *cur_frame_pos, uint16_t *cur_frame_len );

// 扫描接收
void mb_tcp_receive_poll(uint8_t id);
#endif
