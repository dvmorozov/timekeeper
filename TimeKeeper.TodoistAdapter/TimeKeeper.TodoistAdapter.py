
import todoist
import argparse
import configparser

class Task_1:
    def get_Id():
        return 0

    def get_Name():
        return ""

class TodoistClient:
    api = None
    response = None

    def __init__(self, userName, password, **kwargs):
        self.api = todoist.TodoistAPI()
        self.api.user.login(userName, password)
        self.response = self.api.sync()
        return super().__init__(**kwargs)

class WCFAdapter_1_0_0(TodoistClient):
    def GetTaskList(self):
        for item in self.response['items']:
            if item['checked'] == 0:
                s = item['content']
                name = ""
                url = ""
                if s.find('http') == -1:
                    name = s
                else:
                    l = s.split()
                    url = l[0]
                    name = " ".join(l[1:])

                if name[0] == '(':
                    name = name[1:]
                if name[len(name) - 1] == ')':
                    name = name[:len(name) - 1]

                print(name)
                print(url)

    def FinishTask():
        pass

parser = argparse.ArgumentParser(description='Query Todoist API.')
parser.add_argument('--configfile')

def queryTodoistAPI(userName, password):
    adapter = WCFAdapter_1_0_0(userName, password)
    adapter.GetTaskList()

if __name__ == '__main__':
    args = parser.parse_args()

    config = configparser.ConfigParser()
    config.read(args.configfile)

    queryTodoistAPI(config['DEFAULT']['username'], config['DEFAULT']['password'])
