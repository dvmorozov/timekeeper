
import todoist
import argparse
import configparser
import base64
import sys
import win32pipe, win32file

class Task_1:
    id = None
    name = None
    url = None

    def __init__(self, id, name, url, **kwargs):
        self.id = id
        self.name = name
        self.url = url

    def toJSON(self):
        result = '{"id" : "' + str(base64.standard_b64encode(str(self.id).encode()), "utf-8")
        result += '", "name" : "' + str(base64.standard_b64encode(self.name.encode()), "utf-8")
        result += '", "url" : "' + str(base64.standard_b64encode(self.url.encode()), "utf-8") + '"}'
        return result

class TaskList:
    list = []

    def toJSON(self):
        result = '{"tasks" : [' 
        for index in range(0, len(self.list)):
            result += (self.list[index].toJSON())
            if index < len(self.list) - 1:
                result += ', '
        result += ']}'
        return result

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
        result = TaskList()

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

                #  Remove brackets.
                if name[0] == '(':
                    name = name[1:]
                if name[len(name) - 1] == ')':
                    name = name[:len(name) - 1]

                result.list.append(Task_1(item['id'], name, url))

        return result

    def FinishTask():
        pass

parser = argparse.ArgumentParser(description='Todoist API adapter')
parser.add_argument('--configfile')

def queryTodoistAPI(userName, password):
    adapter = WCFAdapter_1_0_0(userName, password)
    list = adapter.GetTaskList()
    return list.toJSON()

if __name__ == '__main__':
    args = parser.parse_args()

    config = configparser.ConfigParser()
    config.read(args.configfile)

    while True:
        p = win32pipe.CreateNamedPipe(r'\\.\pipe\todoist_adapter',
            win32pipe.PIPE_ACCESS_DUPLEX,
            win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_WAIT,
            2, 65536, 65536, 300, None)

        try:
            while True:
                win32pipe.ConnectNamedPipe(p, None)
                data = win32file.ReadFile(p, 4096)

                if data[0] == 0:
                    print('Args: ' + str(data[1:]))
                    win32file.WriteFile(p, bytes(queryTodoistAPI(config['DEFAULT']['username'], config['DEFAULT']['password']), encoding = 'utf-8'))
                    # Wait until all the data will be received by client.
                    win32file.FlushFileBuffers(p)

                else:
                    print('Error: ' + str(data))

                win32pipe.DisconnectNamedPipe(p)

        except Exception as e:
            # When client unexpectedly terminated connection exception occurs.
            print('Exception: ' + str(e))
            win32file.CloseHandle(p)

    print('Process terminated')
    sys.exit(0)
