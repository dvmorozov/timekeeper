
import todoist
import argparse
import configparser
import base64
import sys

class Task_1:
    id = None
    name = None
    url = None

    def __init__(self, id, name, url, **kwargs):
        self.id = id
        self.name = name
        self.url = url

    def toJSON(self):
        result = '{"id" : "' + str(base64.standard_b64encode(str(self.id).encode()))
        result += '", "name" : "' + str(base64.standard_b64encode(self.name.encode()))
        result += '", "url" : "' + str(base64.standard_b64encode(self.url.encode())) + '"}'
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

parser = argparse.ArgumentParser(description='Query Todoist API.')
parser.add_argument('--configfile')
parser.add_argument('--outputfile')

def queryTodoistAPI(userName, password, outFile):
    adapter = WCFAdapter_1_0_0(userName, password)
    list = adapter.GetTaskList()
    file = open(outFile, "w")
    file.write(list.toJSON())
    file.close()

if __name__ == '__main__':
    try:
        args = parser.parse_args()

        config = configparser.ConfigParser()
        config.read(args.configfile)

        queryTodoistAPI(config['DEFAULT']['username'], config['DEFAULT']['password'], args.outputfile)

    except Exception as e:
        print(e)

    finally:
        print('Request completed')
        sys.exit(0)
