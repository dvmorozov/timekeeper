
import todoist
import argparse
import configparser
import base64
import sys
import win32pipe, win32file, win32con

class Task:
    # The list of transmitted attributes.
    def getTransAttrList(self):
        return ["id", "name", "url", "isArchived"]

    def attrToJSON(self, attrName):
        return '"' + attrName +  '": "' + str(base64.standard_b64encode(str(self.__dict__[attrName]).encode()), "utf-8") + '"'

    def toJSON(self):
        result = '{'

        for index in range(0, len(self.getTransAttrList())):
            attrName = self.getTransAttrList()[index]
            result += self.attrToJSON(attrName)
            if index < len(self.getTransAttrList()) - 1:
                result += ', '

        result += '}'
        return result

class TaskList:
    list = []

    def toJSON(self):
        result = '{"tasks" : [' 
        # Convert all tasks into JSON representation.
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
    disableDebug = False

    def __init__(self, userName, password, disableDebug, **kwargs):
        super().__init__(userName, password, **kwargs)
        self.disableDebug = disableDebug

    def GetTaskList(self):
        result = TaskList()

        itemCount = 0
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

                task = Task()
                task.id = item['id']
                task.name = name
                task.url = url
                task.isArchived = bool(item['is_archived'])

                if not self.disableDebug:
                    print('name: ' + name)

                assert len(task.getTransAttrList()) == 4, 'len(task.getTransAttrList()) == 4'
                assert 'id' in task.getTransAttrList(), 'id in task.getTransAttrList()'
                assert 'name' in task.getTransAttrList(), 'name in task.getTransAttrList()'
                assert 'url' in task.getTransAttrList(), 'url in task.getTransAttrList()'
                assert 'isArchived' in task.getTransAttrList(), 'isArchived in task.getTransAttrList()'

                result.list.append(task)
                itemCount += 1

        if not self.disableDebug:
            print('item count: ' + str(itemCount))
        return result

    def FinishTask():
        pass

def queryTodoistAPI(userName, password, disableDebug):
    adapter = WCFAdapter_1_0_0(userName, password, disableDebug)
    list = adapter.GetTaskList()
    return list.toJSON()

def queryWithConfig(disableDebug):
    return queryTodoistAPI(config['DEFAULT']['username'], config['DEFAULT']['password'], disableDebug)

def pipeServer():
    while True:
        try:
            p = win32pipe.CreateNamedPipe(r'\\.\pipe\todoist_adapter',
                win32pipe.PIPE_ACCESS_DUPLEX,
                #win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_WAIT,
                0,
                1, 65536, 65536, 300, None)

            win32pipe.ConnectNamedPipe(p, None)
            data = win32file.ReadFile(p, 4096)

            if data[0] == 0:
                print('Args: ' + str(data[1:]))
                win32file.WriteFile(p, bytes(queryWithConfig(False), encoding = 'utf-8'))
                # Wait until all the data will be received by client.
                #win32file.FlushFileBuffers(p)

            else:
                print('Error: ' + str(data))

            win32file.CloseHandle(p)

        except Exception as e:
            # When client unexpectedly terminated connection exception occurs.
            print('Exception: ' + str(e))
            win32file.CloseHandle(p)

def queryToFile(outFile):
    file = open(outFile, "w")
    file.write(queryWithConfig(False))
    file.close()

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Todoist API adapter')
    parser.add_argument('--configfile')
    parser.add_argument('--outputfile')
    parser.add_argument('--pipe')

    args = parser.parse_args()

    config = configparser.ConfigParser()
    config.read(args.configfile)

    if args.pipe is not None:
        pipeServer()
        print('Process terminated')
    else:
        if args.outputfile is not None:
            queryToFile(args.outputfile)
        else:
            # To standard output.
            print(queryWithConfig(True))

    sys.exit(0)
