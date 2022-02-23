import http from 'k6/http';
import { sleep } from 'k6';
export const options = {
    duration: "60m",
    vus: 3
}

export default function () {
  const url = 'http://localhost:5270/api/student';

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  http.put(url, params);
  sleep(.1);
  http.post(url,params);
  sleep(.1);
  http.get(url,params);
  sleep(.1);
}