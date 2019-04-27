from selenium import webdriver, common
from selenium.webdriver.common.desired_capabilities import DesiredCapabilities
from datetime import datetime
from contextlib import contextmanager
from useragents import get
import signal


def __init_phantom_js_driver():
    dcap = dict(DesiredCapabilities.PHANTOMJS)
    dcap["phantomjs.page.settings.userAgent"] = (str(get()))
    current_date = datetime.isoformat(datetime.now())

    options = webdriver.ChromeOptions()
    options.add_argument('--headless')
    options.add_argument('--no-sandbox')
    options.add_argument('--disable-dev-shm-usage')
    driver = webdriver.Chrome(chrome_options=options,
                              desired_capabilities=dcap,
                              service_log_path='/tmp/phant-' + current_date + '.log',
                              service_args=[]
                              )

    # deprecated
    # driver = webdriver.PhantomJS(
    #     desired_capabilities=dcap,
    #     service_log_path='/tmp/phant-' + current_date + '.log',  # attention!!, can fill all /tmp dir
    #     service_args=[]
    # )

    return driver


@contextmanager
def get_driver(timeout=15):
    try:
        driver = __init_phantom_js_driver()
    except Exception:
        try:
            driver = __init_phantom_js_driver()
        except Exception as e:
            raise DriverInitializationException(
                "Failed to init driver due to {}".format(e)
            )

    driver.set_page_load_timeout(timeout)
    try:
        yield driver
    except common.exceptions.TimeoutException:
        raise DriverTimeoutException(
            "Failed to handle driver, due to page loading timed out!"
        )
    finally:
        driver.service.process.send_signal(signal.SIGTERM)
        driver.quit()


class DriverTimeoutException(Exception):
    """Handles Timeout errors"""

    def __init__(self, msg):
        super(DriverTimeoutException, self).__init__(msg)


class DriverInitializationException(Exception):
    """Handles on-init errors"""

    def __init__(self, msg):
        super(DriverInitializationException, self).__init__(msg)
