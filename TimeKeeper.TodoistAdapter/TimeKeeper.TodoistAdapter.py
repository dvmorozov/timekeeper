
import todoist
import argparse
import configparser

parser = argparse.ArgumentParser(description='Query Todoist API.')
parser.add_argument('--configfile')

def queryTodoistAPI(userName, password):
    api = todoist.TodoistAPI()
    user = api.user.login(userName, password)
    #print(user['full_name'])

    response = api.sync()
    #for project in response['projects']:
    #    print(project['name'])

    for item in response['items']:
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

if __name__ == '__main__':
    args = parser.parse_args()

    config = configparser.ConfigParser()
    config.read(args.configfile)

    queryTodoistAPI(config['DEFAULT']['username'], config['DEFAULT']['password'])
