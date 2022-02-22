
#include <string>
#include <vector>

using namespace std;

int main()
{

}

string solution(vector<string> participant, vector<string> completion) {
    string answer = "";

    for (int i = 0; i < participant.size(); i++)
    {
        for (int j = 0; j < completion.size(); j++)
        {
            if (participant[i] == completion[j])
            {
            }
        }
    }

    return answer;
}