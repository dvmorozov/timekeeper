
import win32pipe
import win32file

if __name__ == '__main__':
    p = win32file.CreateFile("\\\\.\\pipe\\todoist_adapter",
                                  win32file.GENERIC_READ | win32file.GENERIC_WRITE,
                                  0, None,
                                  win32file.OPEN_EXISTING,
                                  0, None)

    data = "Hello Todoist Adapter!"
    win32file.WriteFile(p, bytes(data, encoding = 'utf-8'))

    data = win32file.ReadFile(p, 16384)
    print(str(data))
