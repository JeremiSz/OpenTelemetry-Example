package CCS.OpenTelemetry.M3;

import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class StudentController {

    @RequestMapping(value = "/final", method = RequestMethod.GET)
    public String index() {
        return "hello world";
    }
}
