#include <stdio.h>
#include <stdbool.h>
#include <stdlib.h>

//
//a, b�� ���̴� 1 �̻� 1, 000 �����Դϴ�.
//a, b�� ��� ���� - 1, 000 �̻� 1, 000 �����Դϴ�.
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