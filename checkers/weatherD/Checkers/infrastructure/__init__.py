import sys
if sys.version_info[:2] < (3, 6):
    raise RuntimeError("Python version should be 3.6+")
