#include <stdio.h>
#include <stdbool.h>
#include <stdlib.h>

//
//a, b의 길이는 1 이상 1, 000 이하입니다.
//a, b의 모든 수는 - 1, 000 이상 1, 000 이하입니다.
//
int solution(int a[], size_t a_len, int b[], size_t b_len) {

	int tempSum = 0;
	for (int i = 0; i < a_len; i++)
	{
		tempSum += a[i] * b[i];
	}
	int answer = 1234567890;
	return answer;
}