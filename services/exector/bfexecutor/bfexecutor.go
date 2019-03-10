package bfexector

/*
#cgo CFLAGS: -O0
#cgo LDFLAGS: ${SRCDIR}/lib/bfexecutor.a -lm
#include <bfexecutor.h>
 */
import "C"
import (
	"errors"
	"fmt"
	"unsafe"
)

const MaxOutputLength = 100

func RunBfCode(code, input string, maxOperations uint) (string, error) {
	var bcode = []byte(code)
	codePtr := (*C.char)(unsafe.Pointer(&bcode[0]))
	codeLen := C.int(len(bcode))

	var binput = []byte(input)
	inputPtr := (*C.char)(unsafe.Pointer(&binput[0]))
	inputLen := C.int(len(binput))

	var boutput [MaxOutputLength]byte
	outputPtr := (*C.char)(unsafe.Pointer(&boutput[0]))

	maxOutLen := C.int(MaxOutputLength)

	cmaxOperations := C.uint(maxOperations)

	var writtenBytes uint
	writtenBytesPtr := (*C.uint)(unsafe.Pointer(&writtenBytes))

	retCode := C.run_bf_code(codePtr, codeLen, inputPtr, inputLen, outputPtr, maxOutLen, cmaxOperations, writtenBytesPtr)

	if retCode != 0 {
		return "", errors.New(fmt.Sprintf("Error at code (%v)", retCode))
	}
	writtenBytes = uint(len(boutput))
	fmt.Println(boutput)
	return "", nil
}

