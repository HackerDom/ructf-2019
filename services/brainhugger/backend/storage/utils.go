package storage

import "os"

func CreateDirIfNotExists(dirName string) {
	if _, err := os.Stat(dirName); os.IsNotExist(err) {
		err := os.Mkdir(dirName, 0777)
		if err != nil {
			panic(err)
		}
	}
}
