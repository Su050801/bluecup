#ifndef _INTERRUPT_H_
#define _INTERRUPT_H_


#include"main.h"
#include"stdbool.h"

extern float duty1, duty2;

struct keys
{
	uchar judge_sta;
	bool key_sta;
	bool single_flag;
	uchar long_flag;
	uchar key_time;
};
void HAL_TIM_IC_CaptureCallback(TIM_HandleTypeDef *htim);

#endif
