import type { FC } from "react";
import { Link } from "react-router-dom";

const NotFound: FC = () => {
  return (
    <div className="w-full h-screen relative flex flex-col justify-center items-center">
      <div className="p-5 border-2 rounded-2xl">
        <h1 className="text-4xl font-bold" role="alert">
          ERROR 404: <span className="text-phoenix">Page Not Found</span>
        </h1>
        <p className="text-center">
          Sorry, the page you are looking for could not be found.
        </p>
      </div>
      <br />
      <Link to="/" className="border-2 p-3 font-semibold rounded-2xl">
        Go home
      </Link>
    </div>
  );
};

export default NotFound;
