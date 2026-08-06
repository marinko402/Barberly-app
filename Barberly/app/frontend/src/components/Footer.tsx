import { type FC } from "react";
import { BsCCircle } from "react-icons/bs";
import { FaFacebookSquare, FaLinkedin } from "react-icons/fa";
import { FaSquareInstagram, FaSquareXTwitter } from "react-icons/fa6";
import { NavLink, useNavigate } from "react-router";

const Footer: FC = () => {
  const navigate = useNavigate();

  const mainLinks = [
    { name: "Home", to: "/" },
    { name: "Barbers", to: "/barbers" },
    { name: "About Us", to: "about-us", isSection: true },
  ];

  const serviceLinks = [
    { name: "Haircut", to: "#" },
    { name: "Beard Trim", to: "#" },
    { name: "Hair Styling", to: "#" },
  ];

  const handleScroll = (id: string): void => {
    navigate(`/#${id}`);
  };

  return (
    <footer className="w-full h-fit p-5 pt-12 sm:pt-16 bg-neutral-100 dark:bg-custom-gray border-t border-black/5 dark:border-white/5 overflow-hidden transition-colors duration-300">
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8 max-w-7xl mx-auto px-4 sm:px-10 text-center sm:text-left">
        <div className="flex flex-col items-center sm:items-start min-w-50">
          <h1 className="font-extrabold text-2xl tracking-tight">Contact us</h1>
          <div className="pt-4 pl-8 space-y-2 text-sm sm:text-base text-black/70 dark:text-white/70">
            <p>
              <span className="text-black dark:text-white">
                Customer Support:
              </span>
              <br />9 AM - 6 PM (Mon- Fri)
            </p>
            <p
              className="cursor-pointer hover:text-blue-500 transition-colors"
              onClick={() => {
                window.location.href = "tel:+1234567890";
              }}
            >
              <span className="text-black dark:text-white">Call us:</span> +1
              234 567 890
            </p>
            <p
              className="cursor-pointer hover:text-red-500 transition-colors"
              onClick={() => {
                window.location.href =
                  "mailto:barberly@support.com?subject=Support Request";
              }}
            >
              <span className="text-black dark:text-white">Mail:</span>{" "}
              barberly@support.com
            </p>
          </div>
        </div>

        <div className="flex flex-col items-center sm:items-start min-w-37.5">
          <h1 className="font-extrabold text-2xl tracking-tight">Services</h1>
          <ul className="flex gap-3 flex-col pt-4 text-sm sm:text-base list-none">
            {serviceLinks.map(({ name }, index) => (
              <li key={index}>
                <p className="text-black/50 hover:text-black dark:text-white/50 hover:dark:text-white transition-colors">
                  {name}
                </p>
              </li>
            ))}
          </ul>
        </div>

        <div className="flex flex-col items-center sm:items-start min-w-37.5">
          <h1 className="font-extrabold text-2xl tracking-tight">Barberly</h1>
          <ul className="flex gap-3 flex-col pt-4 text-sm sm:text-base list-none">
            {mainLinks.map(({ name, to, isSection }, index) => (
              <li key={index}>
                {isSection ? (
                  <span
                    onClick={() => handleScroll(to)}
                    className="text-black/50 hover:text-black dark:text-white/50 hover:dark:text-white transition-colors cursor-pointer"
                  >
                    {name}
                  </span>
                ) : (
                  <NavLink
                    to={to}
                    className={({ isActive }) =>
                      isActive
                        ? "text-blue-600 dark:text-blue-400 font-semibold"
                        : "text-black/50 hover:text-black dark:text-white/50 hover:dark:text-white transition-colors"
                    }
                  >
                    {name}
                  </NavLink>
                )}
              </li>
            ))}
          </ul>
        </div>

        <div className="flex flex-col items-center sm:items-start min-w-37.5">
          <h1 className="font-extrabold text-2xl tracking-tight">Find Us</h1>
          <div className="pl-8 flex flex-col gap-2 pt-4 text-sm sm:text-base">
            <a
              href="https://www.instagram.com/"
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-3 text-black/50 hover:text-black dark:text-white/50 hover:dark:text-white transition-colors group cursor-pointer"
            >
              <FaSquareInstagram className="h-5 w-5 text-black/60 dark:text-white/60 group-hover:text-black group-hover:dark:text-white transition-colors" />
              <span>Instagram</span>
            </a>

            <a
              href="https://www.facebook.com/"
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-3 text-black/50 hover:text-black dark:text-white/50 hover:dark:text-white transition-colors group cursor-pointer"
            >
              <FaFacebookSquare className="h-5 w-5 text-black/60 dark:text-white/60 group-hover:text-black group-hover:dark:text-white transition-colors" />
              <span>Facebook</span>
            </a>

            <a
              href="https://www.x.com/"
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-3 text-black/50 hover:text-black dark:text-white/50 hover:dark:text-white transition-colors group cursor-pointer"
            >
              <FaSquareXTwitter className="h-5 w-5 text-black/60 dark:text-white/60 group-hover:text-black group-hover:dark:text-white transition-colors" />
              <span>X</span>
            </a>

            <a
              href="https://www.linkedin.com/"
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-3 text-black/50 hover:text-black dark:text-white/50 hover:dark:text-white transition-colors group cursor-pointer"
            >
              <FaLinkedin className="h-5 w-5 text-black/60 dark:text-white/60 group-hover:text-black group-hover:dark:text-white transition-colors" />
              <span>LinkedIn</span>
            </a>
          </div>
        </div>
      </div>

      <div className="w-full text-center mt-12 sm:mt-16 text-xs sm:text-sm text-black/40 dark:text-white/40">
        <hr className="border-black/5 dark:border-white/5 mb-4" />
        <p className="flex justify-center items-center gap-2 py-2">
          Barberly
          <BsCCircle className="text-[10px] sm:text-xs" />
          by DAMM. All right reserved. {new Date().getFullYear()}.
        </p>
      </div>
    </footer>
  );
};

export default Footer;
